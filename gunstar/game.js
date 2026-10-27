const W = 480, H = 270, T = 32;

// Level layout: '#' ground, '=' metal platform, 'E' enemy spawn, 'G' goal
const LEVEL = [
  '                                                                                          ',
  '                                                                                          ',
  '                                                                                          ',
  '                                    ====                                                  ',
  '                   E          E                       E     ====    E                     ',
  '               =====        =====          E        =====          ====          E     G  ',
  '        E                                ####                                   ####   ## ',
  '###########   ######   #########   ###########   ###########   #########   ###############',
  '###########   ######   #########   ###########   ###########   #########   ###############',
];

class Select extends Phaser.Scene {
  constructor() { super('select'); }
  preload() {
    this.load.spritesheet('jax', 'assets/jax.png', { frameWidth: 32, frameHeight: 32 });
    this.load.spritesheet('zuri', 'assets/zuri.png', { frameWidth: 32, frameHeight: 32 });
    this.load.spritesheet('soldier', 'assets/soldier.png', { frameWidth: 32, frameHeight: 32 });
    this.load.spritesheet('tiles', 'assets/tiles.png', { frameWidth: 32, frameHeight: 32 });
    this.load.spritesheet('fx', 'assets/fx.png', { frameWidth: 32, frameHeight: 32 });
  }
  create() {
    for (const k of ['jax', 'zuri']) {
      this.anims.create({ key: k + '-idle', frames: this.anims.generateFrameNumbers(k, { frames: [0, 1] }), frameRate: 3, repeat: -1 });
      this.anims.create({ key: k + '-run', frames: this.anims.generateFrameNumbers(k, { start: 2, end: 5 }), frameRate: 10, repeat: -1 });
    }
    this.anims.create({ key: 'soldier-walk', frames: this.anims.generateFrameNumbers('soldier', { start: 0, end: 3 }), frameRate: 6, repeat: -1 });
    this.anims.create({ key: 'boom', frames: this.anims.generateFrameNumbers('fx', { start: 2, end: 5 }), frameRate: 16 });

    this.add.text(W / 2, 30, 'GUNSTAR REMIX', { fontSize: '24px', color: '#ffd84a', fontFamily: 'monospace' }).setOrigin(0.5);
    this.add.text(W / 2, 60, 'Choose your hero', { fontSize: '12px', fontFamily: 'monospace' }).setOrigin(0.5);
    const heroes = [['jax', 'JAX', 160], ['zuri', 'ZURI', 320]];
    this.choice = 0;
    this.sprites = heroes.map(([k, name, x]) => {
      this.add.text(x, 190, name, { fontSize: '16px', fontFamily: 'monospace' }).setOrigin(0.5);
      return this.add.sprite(x, 135, k).setScale(3).play(k + '-idle');
    });
    this.cursor = this.add.text(0, 215, '^', { fontSize: '16px', color: '#ffd84a', fontFamily: 'monospace' }).setOrigin(0.5);
    this.add.text(W / 2, 245, 'Left/Right to pick, Z or Enter to start', { fontSize: '10px', fontFamily: 'monospace' }).setOrigin(0.5);
    const pick = (i) => {
      this.choice = i; this.cursor.x = heroes[i][2];
      this.sprites.forEach((s, j) => s.play(heroes[j][0] + (i === j ? '-run' : '-idle'), true));
    };
    pick(0);
    this.input.keyboard.on('keydown-LEFT', () => pick(0));
    this.input.keyboard.on('keydown-RIGHT', () => pick(1));
    const go = () => this.scene.start('play', { hero: heroes[this.choice][0] });
    this.input.keyboard.on('keydown-Z', go);
    this.input.keyboard.on('keydown-ENTER', go);
  }
}

class Play extends Phaser.Scene {
  constructor() { super('play'); }
  init(data) { this.hero = data.hero; }
  create() {
    const cols = LEVEL[0].length;
    this.physics.world.setBounds(0, 0, cols * T, LEVEL.length * T + 200);
    this.cameras.main.setBounds(0, 0, cols * T, LEVEL.length * T).setBackgroundColor('#5fa8e8');
    // simple parallax hills
    for (let i = 0; i < 12; i++) this.add.circle(i * 220, 300, 140, 0x4c8fc4).setScrollFactor(0.3);

    this.solids = this.physics.add.staticGroup();
    this.enemies = this.physics.add.group();
    this.shots = this.physics.add.group({ allowGravity: false });
    this.enemyShots = this.physics.add.group({ allowGravity: false });

    LEVEL.forEach((row, y) => [...row].forEach((ch, x) => {
      const px = x * T + T / 2, py = y * T + T / 2;
      if (ch === '#') this.solids.create(px, py, 'tiles', LEVEL[y - 1]?.[x] === '#' ? 1 : 0);
      if (ch === '=') this.solids.create(px, py, 'tiles', 2);
      if (ch === 'E') this.spawnEnemy(px, py);
      if (ch === 'G') this.goal = this.add.rectangle(px, py - 16, 12, 64, 0xffd84a);
    }));

    const p = this.player = this.physics.add.sprite(64, 150, this.hero);
    p.body.setSize(12, 26).setOffset(10, 4);
    p.hp = 10; p.facing = 1; p.inv = 0; p.cool = 0; p.slide = 0;
    this.cameras.main.startFollow(p, true, 0.15, 0.15);

    this.physics.add.collider(p, this.solids);
    this.physics.add.collider(this.enemies, this.solids);
    this.physics.add.collider(this.shots, this.solids, (s) => s.destroy());
    this.physics.add.collider(this.enemyShots, this.solids, (s) => s.destroy());
    this.physics.add.overlap(this.shots, this.enemies, (s, e) => { s.destroy(); this.hurtEnemy(e, 1); });
    this.physics.add.overlap(p, this.enemyShots, (_, s) => { s.destroy(); this.hurtPlayer(1); });
    this.physics.add.overlap(p, this.enemies, (_, e) => {
      if (p.slide > 0) this.hurtEnemy(e, 3); else this.hurtPlayer(2);
    });

    this.keys = this.input.keyboard.addKeys('LEFT,RIGHT,UP,DOWN,Z,X,C');
    this.hud = this.add.text(8, 8, '', { fontSize: '12px', fontFamily: 'monospace', color: '#fff' }).setScrollFactor(0);
    this.over = false;
  }

  spawnEnemy(x, y) {
    const e = this.enemies.create(x, y, 'soldier').play('soldier-walk');
    e.body.setSize(14, 26).setOffset(9, 4);
    e.hp = 3; e.dir = -1; e.fire = Phaser.Math.Between(60, 140);
  }

  shoot(dx, dy) {
    const p = this.player, len = Math.hypot(dx, dy);
    const s = this.shots.create(p.x + dx * 12, p.y - 4 + dy * 12, 'fx', 0);
    s.rotation = Math.atan2(dy, dx);
    s.body.setSize(8, 8).setVelocity(dx / len * 420, dy / len * 420);
    this.time.delayedCall(900, () => s.destroy());
  }

  hurtEnemy(e, dmg) {
    if (!e.active) return;
    e.hp -= dmg; e.setTint(0xffffff).setTintFill(0xffffff);
    this.time.delayedCall(60, () => e.active && e.clearTint());
    if (e.hp <= 0) { this.boom(e.x, e.y); e.destroy(); }
  }

  hurtPlayer(dmg) {
    const p = this.player;
    if (p.inv > 0 || this.over) return;
    p.hp -= dmg; p.inv = 60;
    p.setVelocity(-p.facing * 150, -180);
    this.cameras.main.shake(100, 0.01);
    if (p.hp <= 0) this.end('GAME OVER');
  }

  boom(x, y) {
    const b = this.add.sprite(x, y, 'fx', 2).setScale(1.5).play('boom');
    b.on('animationcomplete', () => b.destroy());
  }

  end(msg) {
    this.over = true;
    this.player.setVelocity(0).body.enable = false;
    this.add.text(W / 2, H / 2, msg + '\nPress Z', { fontSize: '24px', fontFamily: 'monospace', align: 'center', color: '#ffd84a' })
      .setOrigin(0.5).setScrollFactor(0);
    this.input.keyboard.once('keydown-Z', () => this.scene.start('select'));
  }

  update() {
    if (this.over) return;
    const p = this.player, k = this.keys, onGround = p.body.blocked.down;
    const h = (k.RIGHT.isDown ? 1 : 0) - (k.LEFT.isDown ? 1 : 0);
    const v = (k.DOWN.isDown ? 1 : 0) - (k.UP.isDown ? 1 : 0);
    const planted = k.C.isDown && onGround;
    p.inv = Math.max(0, p.inv - 1); p.cool--;
    p.setAlpha(p.inv > 0 && p.inv % 6 < 3 ? 0.4 : 1);

    if (p.slide > 0) {
      p.slide--; p.setVelocityX(p.facing * 300);
    } else if (onGround && k.DOWN.isDown && Phaser.Input.Keyboard.JustDown(k.Z)) {
      p.slide = 20;
    } else {
      if (h) p.facing = h;
      p.setVelocityX(planted ? 0 : h * 150);
      if (onGround && Phaser.Input.Keyboard.JustDown(k.Z)) p.setVelocityY(-380);
    }
    p.setFlipX(p.facing < 0);

    // 8-way aim: while planted, or aiming up/down in the air
    if (k.X.isDown && p.cool <= 0 && p.slide === 0) {
      let ax = h, ay = v;
      if (onGround && !planted && ay > 0) ay = 0;  // down on ground isn't an aim
      if (!ax && !ay) ax = p.facing;
      this.shoot(ax, ay); p.cool = 7;
    }

    const anim = p.inv > 40 ? 8 : p.slide > 0 ? 7 : !onGround ? 6 : null;
    if (anim !== null) { p.anims.stop(); p.setFrame(anim); }
    else p.play(this.hero + (p.body.velocity.x ? '-run' : '-idle'), true);
    p.body.setSize(12, p.slide > 0 ? 12 : 26).setOffset(10, p.slide > 0 ? 18 : 4);

    for (const e of this.enemies.getChildren()) {
      if (Math.abs(e.x - p.x) > W) continue;   // dormant offscreen
      const ahead = this.solids.getChildren().some(t => Math.abs(t.x - (e.x + e.dir * 16)) < 16 && Math.abs(t.y - (e.y + 32)) < 16);
      if (e.body.blocked.down && (!ahead || e.body.blocked.left || e.body.blocked.right)) e.dir *= -1;
      e.setVelocityX(e.dir * 40).setFlipX(e.dir > 0);
      if (--e.fire <= 0) {
        e.fire = Phaser.Math.Between(90, 160);
        const dir = Math.sign(p.x - e.x) || 1; e.dir = dir; e.setFlipX(dir > 0);
        const s = this.enemyShots.create(e.x + dir * 14, e.y - 1, 'fx', 1);
        s.body.setSize(6, 6).setVelocityX(dir * 160);
        this.time.delayedCall(2500, () => s.destroy());
      }
    }

    if (p.y > LEVEL.length * T + 40) { p.hp = 0; this.end('GAME OVER'); }
    if (this.goal && Math.abs(p.x - this.goal.x) < 16) this.end('STAGE CLEAR!');
    this.hud.setText(`${this.hero.toUpperCase()}  HP ${'|'.repeat(Math.max(0, p.hp))}   ENEMIES ${this.enemies.countActive()}`);
  }
}

new Phaser.Game({
  type: Phaser.AUTO, parent: 'game', width: W, height: H, pixelArt: true,
  scale: { mode: Phaser.Scale.FIT, autoCenter: Phaser.Scale.CENTER_HORIZONTALLY, width: W, height: H },
  physics: { default: 'arcade', arcade: { gravity: { y: 900 } } },
  scene: [Select, Play],
});
