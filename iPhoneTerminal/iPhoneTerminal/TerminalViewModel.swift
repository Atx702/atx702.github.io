import Foundation
import SwiftUI

@MainActor
final class TerminalViewModel: ObservableObject {
    @Published var lines: [TerminalLine] = []
    @Published var errorMessage: String?

    private let shell = ShellProcess()
    private let parser = ANSIParser()
    private let maxScrollback = 2000

    init() {
        shell.onOutput = { [weak self] data in
            self?.handleOutput(data)
        }
        shell.onExit = { [weak self] in
            self?.appendPlain("\n[Process exited]", color: .red)
        }
        startShell()
    }

    private func startShell() {
        do {
            try shell.start()
            appendPlain("iPhoneTerminal — /bin/sh\n", color: .green)
        } catch {
            errorMessage = error.localizedDescription
            appendPlain("Error: \(error.localizedDescription)\n", color: .red)
        }
    }

    // MARK: - Input

    func send(_ text: String) {
        shell.writeString(text)
    }

    func sendControl(_ char: Character) {
        guard let ascii = char.asciiValue else { return }
        let ctrl = ascii & 0x1F
        shell.write(Data([ctrl]))
    }

    func sendArrow(_ direction: ArrowKey) {
        shell.writeString("\u{1B}[\(direction.code)")
    }

    func sendTab() { shell.write(Data([0x09])) }
    func sendEscape() { shell.write(Data([0x1B])) }

    func resize(cols: UInt16, rows: UInt16) {
        shell.resize(cols: cols, rows: rows)
    }

    // MARK: - Output

    private func handleOutput(_ data: Data) {
        let results = parser.parse(data)
        for result in results {
            if result.clearScreen {
                lines.removeAll()
            }
            if result.carriageReturn {
                // Erase the last line's content (CR overwrites current line)
                if !lines.isEmpty { lines[lines.count - 1].segments.removeAll() }
            }
            for segment in result.segments {
                appendSegment(segment)
            }
        }
        trimScrollback()
    }

    private func appendSegment(_ segment: StyledText) {
        // Split on newlines, placing each piece on its own line
        let parts = segment.text.components(separatedBy: "\n")
        for (i, part) in parts.enumerated() {
            if i > 0 { lines.append(TerminalLine()) }
            if lines.isEmpty { lines.append(TerminalLine()) }
            var s = segment
            s.text = part
            lines[lines.count - 1].append(s)
        }
    }

    private func appendPlain(_ text: String, color: Color = .white) {
        appendSegment(StyledText(text, fg: color))
    }

    private func trimScrollback() {
        if lines.count > maxScrollback {
            lines.removeFirst(lines.count - maxScrollback)
        }
    }
}

enum ArrowKey: String {
    case up, down, left, right
    var code: String {
        switch self {
        case .up:    return "A"
        case .down:  return "B"
        case .right: return "C"
        case .left:  return "D"
        }
    }
}
