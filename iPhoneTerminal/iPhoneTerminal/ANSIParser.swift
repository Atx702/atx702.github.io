import SwiftUI

// MARK: - Types

struct StyledText {
    var text: String
    var foreground: Color
    var background: Color
    var bold: Bool
    var underline: Bool

    init(_ text: String, fg: Color = .white, bg: Color = .clear, bold: Bool = false, underline: Bool = false) {
        self.text = text
        self.foreground = fg
        self.background = bg
        self.bold = bold
        self.underline = underline
    }
}

struct TerminalLine {
    var segments: [StyledText] = []
    var id = UUID()

    var plainText: String { segments.map(\.text).joined() }
    var isEmpty: Bool { segments.isEmpty || plainText.isEmpty }

    mutating func append(_ segment: StyledText) {
        if !segment.text.isEmpty { segments.append(segment) }
    }
}

// MARK: - Parser

/// Stateful ANSI escape sequence parser.
/// Handles SGR (colors/formatting), ED (erase display), EL (erase line), CUP (cursor position).
final class ANSIParser {

    struct Style {
        var foreground: Color = .white
        var background: Color = .clear
        var bold: Bool = false
        var underline: Bool = false
    }

    private var style = Style()

    // Leftover bytes from a partial escape sequence
    private var pending = Data()

    struct ParseResult {
        var segments: [StyledText]
        var clearScreen: Bool
        var carriageReturn: Bool
    }

    func parse(_ data: Data) -> [ParseResult] {
        var combined = pending + data
        pending = Data()

        var results: [ParseResult] = []
        var currentSegments: [StyledText] = []
        var clearScreen = false
        var carriageReturn = false

        var i = combined.startIndex

        func flush() {
            if !currentSegments.isEmpty || clearScreen || carriageReturn {
                results.append(ParseResult(segments: currentSegments,
                                           clearScreen: clearScreen,
                                           carriageReturn: carriageReturn))
                currentSegments = []
                clearScreen = false
                carriageReturn = false
            }
        }

        func addText(_ s: String) {
            if s.isEmpty { return }
            currentSegments.append(StyledText(s, fg: style.foreground, bg: style.background,
                                               bold: style.bold, underline: style.underline))
        }

        while i < combined.endIndex {
            let byte = combined[i]

            if byte == 0x1B { // ESC
                let remaining = combined.distance(from: i, to: combined.endIndex)
                if remaining < 2 {
                    // Incomplete — save for next parse call
                    pending = combined[i...]
                    break
                }
                let next = combined[combined.index(after: i)]
                if next == 0x5B { // ESC [
                    // Collect until we hit a final byte (0x40–0x7E)
                    var j = combined.index(i, offsetBy: 2)
                    while j < combined.endIndex && (combined[j] < 0x40 || combined[j] > 0x7E) {
                        j = combined.index(after: j)
                    }
                    if j >= combined.endIndex {
                        pending = combined[i...]
                        break
                    }
                    let finalByte = combined[j]
                    let paramRange = combined.index(i, offsetBy: 2)..<j
                    let paramStr = String(bytes: combined[paramRange], encoding: .utf8) ?? ""

                    switch finalByte {
                    case 0x6D: // m — SGR
                        applySGR(paramStr)
                    case 0x4A: // J — ED (erase display)
                        if paramStr == "2" || paramStr == "" || paramStr == "0" {
                            flush()
                            clearScreen = true
                        }
                    case 0x4B: // K — EL (erase line) — treat as newline for simplicity
                        break
                    case 0x48, 0x66: // H / f — CUP cursor position, skip
                        break
                    case 0x41, 0x42, 0x43, 0x44: // A/B/C/D cursor movement, skip
                        break
                    default:
                        break
                    }
                    i = combined.index(after: j)
                } else {
                    // Non-CSI escape, skip
                    i = combined.index(after: combined.index(after: i))
                }
                continue
            }

            if byte == 0x0D { // CR
                flush()
                carriageReturn = true
                i = combined.index(after: i)
                continue
            }

            if byte == 0x07 || byte == 0x08 || byte == 0x0C || byte == 0x0E || byte == 0x0F {
                // Bell, BS, FF, SO, SI — skip
                i = combined.index(after: i)
                continue
            }

            // Collect a run of plain UTF-8 text
            var runEnd = i
            while runEnd < combined.endIndex {
                let b = combined[runEnd]
                if b == 0x1B || b == 0x0D || b == 0x07 || b == 0x08 { break }
                runEnd = combined.index(after: runEnd)
            }
            if let s = String(bytes: combined[i..<runEnd], encoding: .utf8) {
                addText(s)
            }
            i = runEnd
        }

        flush()
        return results
    }

    // MARK: SGR

    private func applySGR(_ params: String) {
        let codes = params.isEmpty ? [0] : params.split(separator: ";").compactMap { Int($0) }
        var idx = 0
        while idx < codes.count {
            let code = codes[idx]
            switch code {
            case 0:
                style = Style()
            case 1:
                style.bold = true
            case 4:
                style.underline = true
            case 22:
                style.bold = false
            case 24:
                style.underline = false
            case 30...37:
                style.foreground = ansiColor(code - 30, bright: false)
            case 39:
                style.foreground = .white
            case 40...47:
                style.background = ansiColor(code - 40, bright: false)
            case 49:
                style.background = .clear
            case 90...97:
                style.foreground = ansiColor(code - 90, bright: true)
            case 100...107:
                style.background = ansiColor(code - 100, bright: true)
            case 38:
                // 256-color: 38;5;n or 38;2;r;g;b
                if idx + 2 < codes.count && codes[idx + 1] == 5 {
                    style.foreground = color256(codes[idx + 2])
                    idx += 2
                } else if idx + 4 < codes.count && codes[idx + 1] == 2 {
                    style.foreground = Color(red: Double(codes[idx+2])/255,
                                             green: Double(codes[idx+3])/255,
                                             blue: Double(codes[idx+4])/255)
                    idx += 4
                }
            case 48:
                if idx + 2 < codes.count && codes[idx + 1] == 5 {
                    style.background = color256(codes[idx + 2])
                    idx += 2
                } else if idx + 4 < codes.count && codes[idx + 1] == 2 {
                    style.background = Color(red: Double(codes[idx+2])/255,
                                              green: Double(codes[idx+3])/255,
                                              blue: Double(codes[idx+4])/255)
                    idx += 4
                }
            default:
                break
            }
            idx += 1
        }
    }

    private func ansiColor(_ index: Int, bright: Bool) -> Color {
        let colors: [Color] = bright ? [
            Color(white: 0.5),      // bright black / dark gray
            Color(red: 1, green: 0.3, blue: 0.3),
            Color(red: 0.3, green: 1, blue: 0.3),
            Color(red: 1, green: 1, blue: 0.3),
            Color(red: 0.3, green: 0.3, blue: 1),
            Color(red: 1, green: 0.3, blue: 1),
            Color(red: 0.3, green: 1, blue: 1),
            .white
        ] : [
            .black,
            Color(red: 0.8, green: 0.1, blue: 0.1),
            Color(red: 0.1, green: 0.7, blue: 0.1),
            Color(red: 0.7, green: 0.6, blue: 0.1),
            Color(red: 0.1, green: 0.1, blue: 0.8),
            Color(red: 0.7, green: 0.1, blue: 0.7),
            Color(red: 0.1, green: 0.7, blue: 0.7),
            Color(white: 0.8)
        ]
        return index < colors.count ? colors[index] : .white
    }

    private func color256(_ n: Int) -> Color {
        if n < 8 { return ansiColor(n, bright: false) }
        if n < 16 { return ansiColor(n - 8, bright: true) }
        if n < 232 {
            let v = n - 16
            let b = v % 6; let g = (v / 6) % 6; let r = v / 36
            return Color(red: Double(r == 0 ? 0 : 55 + r*40)/255,
                         green: Double(g == 0 ? 0 : 55 + g*40)/255,
                         blue: Double(b == 0 ? 0 : 55 + b*40)/255)
        }
        let gray = Double(8 + (n - 232) * 10) / 255
        return Color(white: gray)
    }
}
