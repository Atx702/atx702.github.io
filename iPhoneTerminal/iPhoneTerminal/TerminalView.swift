import SwiftUI

struct TerminalView: View {
    @ObservedObject var terminal: TerminalViewModel
    @State private var inputText = ""
    @State private var scrollProxy: ScrollViewProxy?
    @FocusState private var inputFocused: Bool

    private let font = Font.system(size: 13, design: .monospaced)
    private let bottomID = "bottom"

    var body: some View {
        VStack(spacing: 0) {
            // Output area
            ScrollViewReader { proxy in
                ScrollView(.vertical, showsIndicators: true) {
                    LazyVStack(alignment: .leading, spacing: 0) {
                        ForEach(terminal.lines) { line in
                            LineView(line: line, font: font)
                        }
                        Color.clear.frame(height: 1).id(bottomID)
                    }
                    .padding(.horizontal, 6)
                    .padding(.top, 6)
                }
                .onAppear { scrollProxy = proxy }
                .onChange(of: terminal.lines.count) { _ in
                    withAnimation(.none) {
                        proxy.scrollTo(bottomID, anchor: .bottom)
                    }
                }
            }

            Divider().background(Color.gray.opacity(0.4))

            // Keyboard toolbar
            KeyboardToolbar(terminal: terminal, inputText: $inputText)

            // Input bar
            HStack(spacing: 6) {
                Text("$")
                    .foregroundColor(.green)
                    .font(font)

                TextField("", text: $inputText)
                    .font(font)
                    .foregroundColor(.white)
                    .accentColor(.green)
                    .autocapitalization(.none)
                    .autocorrectionDisabled(true)
                    .keyboardType(.asciiCapable)
                    .focused($inputFocused)
                    .submitLabel(.send)
                    .onSubmit { commitInput() }

                Button(action: commitInput) {
                    Image(systemName: "return")
                        .foregroundColor(.green)
                }
            }
            .padding(.horizontal, 10)
            .padding(.vertical, 8)
            .background(Color(white: 0.08))
        }
        .background(Color.black)
        .onAppear { inputFocused = true }
    }

    private func commitInput() {
        terminal.send(inputText + "\n")
        inputText = ""
    }
}

// MARK: - Line rendering

private struct LineView: View {
    let line: TerminalLine
    let font: Font

    var body: some View {
        if line.segments.isEmpty {
            Text(" ").font(font) // preserve blank lines
        } else {
            line.segments.reduce(Text("")) { acc, seg in
                acc + styledText(seg)
            }
            .font(font)
            .fixedSize(horizontal: false, vertical: true)
        }
    }

    private func styledText(_ seg: StyledText) -> Text {
        var t = Text(seg.text)
            .foregroundColor(seg.foreground)
        if seg.bold { t = t.bold() }
        if seg.underline { t = t.underlineStyle(style: .single) }
        if seg.background != .clear {
            t = t.background(seg.background)
        }
        return t
    }
}

// MARK: - Text underline helper

private extension Text {
    func underlineStyle(style: NSUnderlineStyle) -> Text {
        self.underline()
    }
}

// MARK: - Keyboard toolbar

struct KeyboardToolbar: View {
    @ObservedObject var terminal: TerminalViewModel
    @Binding var inputText: String

    private let font = Font.system(size: 13, weight: .medium, design: .monospaced)

    var body: some View {
        ScrollView(.horizontal, showsIndicators: false) {
            HStack(spacing: 6) {
                key("Tab")    { terminal.sendTab() }
                key("Ctrl+C") { terminal.sendControl("c") }
                key("Ctrl+D") { terminal.sendControl("d") }
                key("Ctrl+Z") { terminal.sendControl("z") }
                key("Ctrl+L") { terminal.sendControl("l") }
                key("Esc")    { terminal.sendEscape() }
                key("↑")      { terminal.sendArrow(.up) }
                key("↓")      { terminal.sendArrow(.down) }
                key("←")      { terminal.sendArrow(.left) }
                key("→")      { terminal.sendArrow(.right) }
                key("Ctrl+A") { terminal.sendControl("a") }
                key("Ctrl+E") { terminal.sendControl("e") }
                key("Ctrl+K") { terminal.sendControl("k") }
                key("Ctrl+U") { terminal.sendControl("u") }
                key("Ctrl+W") { terminal.sendControl("w") }
                key("Ctrl+R") { terminal.sendControl("r") }
            }
            .padding(.horizontal, 8)
            .padding(.vertical, 6)
        }
        .background(Color(white: 0.12))
    }

    @ViewBuilder
    private func key(_ label: String, action: @escaping () -> Void) -> some View {
        Button(action: action) {
            Text(label)
                .font(font)
                .foregroundColor(.white)
                .padding(.horizontal, 10)
                .padding(.vertical, 6)
                .background(Color(white: 0.22))
                .cornerRadius(6)
        }
    }
}
