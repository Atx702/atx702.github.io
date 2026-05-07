import Foundation

/// Manages a PTY-backed shell process.
/// On a sideloaded app with Developer Mode enabled, iOS allows spawning /bin/sh via posix_spawn.
final class ShellProcess {
    var onOutput: ((Data) -> Void)?
    var onExit: (() -> Void)?

    private var masterFD: Int32 = -1
    private var pid: pid_t = -1
    private var readThread: Thread?

    func start() throws {
        // Open PTY master
        masterFD = posix_openpt(O_RDWR | O_NOCTTY)
        guard masterFD >= 0 else { throw ShellError.ptyOpenFailed(errno) }

        guard grantpt(masterFD) == 0, unlockpt(masterFD) == 0 else {
            close(masterFD)
            throw ShellError.ptyOpenFailed(errno)
        }

        guard let slavePath = ptsname(masterFD) else {
            close(masterFD)
            throw ShellError.ptyOpenFailed(errno)
        }

        let slaveFD = open(slavePath, O_RDWR | O_NOCTTY)
        guard slaveFD >= 0 else {
            close(masterFD)
            throw ShellError.ptyOpenFailed(errno)
        }

        // Set window size (default 80x24, updated later via resize())
        var ws = winsize()
        ws.ws_col = 80
        ws.ws_row = 24
        ioctl(slaveFD, UInt(TIOCSWINSZ), &ws)

        // Wire slave PTY as stdin/stdout/stderr for the child
        var fileActions: posix_spawn_file_actions_t?
        posix_spawn_file_actions_init(&fileActions)
        defer { posix_spawn_file_actions_destroy(&fileActions) }

        posix_spawn_file_actions_adddup2(&fileActions, slaveFD, STDIN_FILENO)
        posix_spawn_file_actions_adddup2(&fileActions, slaveFD, STDOUT_FILENO)
        posix_spawn_file_actions_adddup2(&fileActions, slaveFD, STDERR_FILENO)
        posix_spawn_file_actions_addclose(&fileActions, slaveFD)
        posix_spawn_file_actions_addclose(&fileActions, masterFD)

        var spawnAttrs: posix_spawnattr_t?
        posix_spawnattr_init(&spawnAttrs)
        defer { posix_spawnattr_destroy(&spawnAttrs) }
        posix_spawnattr_setflags(&spawnAttrs, Int16(POSIX_SPAWN_SETSIGDEF | POSIX_SPAWN_SETSIGMASK))

        let shell = "/bin/sh"
        let argv = [shell, "-i"].map { strdup($0) } + [nil as UnsafeMutablePointer<CChar>?]

        var envPairs = ProcessInfo.processInfo.environment
        envPairs["TERM"] = "xterm-256color"
        let envp = envPairs.map { strdup("\($0.key)=\($0.value)") } + [nil as UnsafeMutablePointer<CChar>?]
        defer {
            argv.forEach { free($0) }
            envp.forEach { free($0) }
        }

        let ret = posix_spawn(&pid, shell,
                              &fileActions, &spawnAttrs,
                              argv.map { $0 },
                              envp.map { $0 })
        close(slaveFD)

        if ret != 0 {
            close(masterFD)
            masterFD = -1
            throw ShellError.spawnFailed(ret)
        }

        startReadLoop()
    }

    private func startReadLoop() {
        readThread = Thread { [weak self] in
            guard let self else { return }
            var buf = [UInt8](repeating: 0, count: 4096)
            while true {
                let n = read(self.masterFD, &buf, buf.count)
                if n <= 0 {
                    DispatchQueue.main.async { self.onExit?() }
                    break
                }
                let data = Data(buf[0..<n])
                DispatchQueue.main.async { self.onOutput?(data) }
            }
        }
        readThread?.qualityOfService = .userInteractive
        readThread?.start()
    }

    func write(_ data: Data) {
        guard masterFD >= 0 else { return }
        data.withUnsafeBytes { ptr in
            _ = Foundation.write(masterFD, ptr.baseAddress!, data.count)
        }
    }

    func writeString(_ s: String) {
        guard let d = s.data(using: .utf8) else { return }
        write(d)
    }

    func resize(cols: UInt16, rows: UInt16) {
        guard masterFD >= 0 else { return }
        var ws = winsize()
        ws.ws_col = cols
        ws.ws_row = rows
        ioctl(masterFD, UInt(TIOCSWINSZ), &ws)
    }

    func terminate() {
        if pid > 0 { kill(pid, SIGKILL); pid = -1 }
        if masterFD >= 0 { close(masterFD); masterFD = -1 }
    }

    deinit { terminate() }
}

enum ShellError: LocalizedError {
    case ptyOpenFailed(Int32)
    case spawnFailed(Int32)

    var errorDescription: String? {
        switch self {
        case .ptyOpenFailed(let e): return "PTY open failed: \(String(cString: strerror(e)))"
        case .spawnFailed(let e):   return "posix_spawn failed: \(String(cString: strerror(e)))"
        }
    }
}
