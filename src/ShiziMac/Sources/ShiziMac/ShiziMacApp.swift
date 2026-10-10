import AppKit
import SwiftUI

@main
struct ShiziMacApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) private var appDelegate

    var body: some Scene {
        Window("拾字 Shizi", id: "main") {
            ContentView(model: appDelegate.model)
                .frame(minWidth: 760, minHeight: 520)
        }
        .defaultSize(width: 820, height: 560)
        .windowResizability(.contentMinSize)

        MenuBarExtra("拾字", systemImage: "viewfinder") {
            MenuBarContent(model: appDelegate.model)
        }
    }
}

@MainActor
final class AppDelegate: NSObject, NSApplicationDelegate {
    let model = AppModel()
    private var hotKey: GlobalHotKey?

    func applicationDidFinishLaunching(_ notification: Notification) {
        NSApp.setActivationPolicy(.regular)
        hotKey = GlobalHotKey { [weak model] in
            model?.startCapture()
        }
    }

    func applicationShouldTerminateAfterLastWindowClosed(_ sender: NSApplication) -> Bool {
        false
    }
}

private struct MenuBarContent: View {
    @ObservedObject var model: AppModel
    @Environment(\.openWindow) private var openWindow

    var body: some View {
        Button("开始取字    ⌥⇧A") { model.startCapture() }
        Divider()
        Button("打开拾字") {
            openWindow(id: "main")
            NSApp.activate(ignoringOtherApps: true)
        }
        Toggle("登录时自动运行", isOn: Binding(
            get: { model.launchAtLogin },
            set: { model.setLaunchAtLogin($0) }
        ))
        Divider()
        Button("退出拾字") { NSApp.terminate(nil) }
    }
}
