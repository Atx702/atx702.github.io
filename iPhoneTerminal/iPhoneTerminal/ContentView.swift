import SwiftUI

struct ContentView: View {
    @StateObject private var terminal = TerminalViewModel()

    var body: some View {
        TerminalView(terminal: terminal)
            .ignoresSafeArea(.container, edges: .top)
            .background(Color.black)
    }
}
