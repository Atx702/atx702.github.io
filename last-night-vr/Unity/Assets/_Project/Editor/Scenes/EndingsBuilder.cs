using System.Collections.Generic;
using LastNight.Chapters;
using LastNight.Core;
using LastNight.Player;
using LastNight.UI;
using UnityEngine;
using G = LastNight.EditorTools.Rooms.Gap;

namespace LastNight.EditorTools
{
    /// <summary>
    /// The secret ending ("Maren's Words"): End_Morning.unity is the hospital room two days later;
    /// End_MorningField.unity is the field at morning with a sailboat on a pond while the credits drift by.
    /// </summary>
    public static class EndingsBuilder
    {
        public static void BuildHospitalRoom()
        {
            var scene = Kit.NewScene("End_Morning");
            Kit.Atmosphere(Kit.Mood(new Color(0.85f, 0.88f, 0.86f), 8f, 30f, new Color(0.62f, 0.66f, 0.66f), new Color(1f, 0.95f, 0.85f), 0.8f, new Vector3(25, 140, 0)));
            var r = Kit.Go("Hospital Room").transform;
            Rooms.Box("Room", r, new Vector3(-2.5f, 0, -1f), new Vector2(5f, 5f), 2.7f, new Color(0.82f, 0.87f, 0.86f), Kit.C("linoleum"), ceilingColor: Color.white);
            Kit.Block("Window", r, new Vector3(2.48f, 1.5f, 1.5f), new Vector3(0.02f, 1.3f, 2f), Color.white, false)
                .GetComponent<Renderer>().sharedMaterial = Kit.Mat(new Color(1f, 0.95f, 0.85f), 2f);
            Kit.PointLight("Morning", r, new Vector3(1.8f, 2f, 1.5f), new Color(1f, 0.93f, 0.8f), 1.5f, 7f);
            var bed = Kit.Model("Hospital", "Gurney", r, new Vector3(0, 0, 1.2f), 0f, length: 1.9f, rootName: "Bed");
            Kit.Spawn(r, new Vector3(-0.2f, 0, 0.2f), 20f);
            var marenPuppet = Kit.Character("Maren_tonight", r, new Vector3(1.2f, 0, 1.0f), -90f, Characters.PuppetAnimator.Move.SitTalk, 0.8f, "Maren");
            var maren = marenPuppet.gameObject;
            Kit.Model("Hospital", "Waiting_chair", r, new Vector3(1.25f, 0, 1.0f), -90f, height: 0.95f);
            var speaker = Kit.Go("Maren Speaker", maren.transform, new Vector3(0, 1.3f, 0));
            var monitor = Kit.Sound(r, "Monitor", new Vector3(-0.9f, 1.4f, 2.2f), "SFX_monitor_beep", loop: true, volume: 0.35f);
            Kit.Model("Bedroom_Wren_s_room", "Defibrillator_monitor", r, new Vector3(-0.9f, 1.0f, 2.2f), 180f, length: 0.4f);
            var music = Kit.Sound(r, "Morning Music", Vector3.zero, "MUS_Morning", spatial: false, volume: 0.8f);

            var d = Kit.Go("Director").AddComponent<SecretEndingDirector>();
            Kit.Set(d, "chapter", Chapter.LittleLantern);
            Kit.Set(d, "maren", speaker.transform);
            Kit.Set(d, "marenPuppet", marenPuppet);
            Kit.Set(d, "handsOut", Kit.Gesture(d.transform, GestureDetector.Kind.HandsOut, d.OnHandOut));
            Kit.Set(d, "monitorSteady", monitor);
            Kit.Set(d, "morningMusic", music);
            Kit.Set(d, "morningFieldScene", "End_MorningField");
            Kit.Set(d, "creditsDelay", 80f);
            Kit.Save(scene, "End_Morning");
        }

        static readonly string[] Credits =
        {
            "LAST NIGHT",
            "Tomas Reyes\nMaren Reyes\nWren Reyes",
            "Story, script and design\nwritten with Claude",
            "Voices\nElevenLabs",
            "Music and sound\nElevenLabs",
            "Models\nMeshy, and made by hand in code",
            "Built in Unity\nfor Meta Quest",
            "For everyone who was told\nto keep breathing.",
            "Little lantern, little light,\nkeep me company tonight.",
        };

        public static void BuildMorningField()
        {
            var scene = Kit.NewScene("End_MorningField");
            Kit.Atmosphere(Kit.Mood(new Color(0.88f, 0.9f, 0.92f), 30f, 220f, new Color(0.7f, 0.72f, 0.7f), new Color(1f, 0.92f, 0.78f), 1.1f, new Vector3(15, 100, 0)));
            var w = Kit.Go("Morning Field").transform;
            Kit.Floor("Ground", w, Vector3.zero, new Vector2(400, 400), new Color(0.52f, 0.6f, 0.42f));
            Kit.Tufts(w, Vector3.zero, 2f, 80f, 260, new Color(0.45f, 0.55f, 0.35f), 7);
            Kit.Spawn(w, Vector3.zero, 0f);
            Kit.Block("Pond", w, new Vector3(0, 0.02f, 9f), new Vector3(8f, 0.04f, 6f), new Color(0.45f, 0.6f, 0.72f), false);
            var boat = Kit.Model("Prologue_Field", "Toy_sailboat", w, new Vector3(-1f, 0.04f, 9f), 70f, length: 0.5f, rootName: "Sailboat");
            Cut.New("Sailboat Drift", boat, 80f).Move(boat.transform, 0f, 80f, new Vector3(-2.5f, 0.04f, 9f), new Vector3(2.5f, 0.04f, 10f), null, null, 0.01f, 0.4f).Done()
                .playOnAwake = true;
            Kit.Sound(w, "Birds", Vector3.zero, "SFX_wind_chimes", loop: true, spatial: false, volume: 0.2f, playOnAwake: true);

            var pages = new List<Transform>();
            var holder = Kit.Go("Credits", w);
            foreach (var text in Credits)
            {
                var page = UiKit.Panel("Page", holder.transform, new Vector3(0, 1.6f, 20f), 0f, new Vector2(560, 380), UiKit.Paper);
                UiKit.Label(page.transform, text, Vector2.zero, new Vector2(500, 330), 36f);
                pages.Add(page.transform);
            }
            var roll = holder.AddComponent<CreditsRoll>();
            Kit.Set(roll, "pages", pages);
            Kit.Set(roll, "speed", 1.2f);
            Kit.Set(roll, "spacing", 8f);
            Kit.Set(roll, "returnToMenuWhenDone", false); // the GameManager returns to the bed after the music
            Kit.Save(scene, "End_MorningField");
        }
    }
}
