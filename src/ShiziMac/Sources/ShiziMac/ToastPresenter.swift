import AppKit

@MainActor
enum ToastPresenter {
    private static var panel: NSPanel?

    static func show(_ text: String) {
        panel?.close()

        let label = NSTextField(labelWithString: text)
        label.font = .systemFont(ofSize: 14, weight: .semibold)
        label.textColor = .white
        label.alignment = .center

        let visual = NSVisualEffectView(frame: NSRect(x: 0, y: 0, width: 160, height: 52))
        visual.material = .hudWindow
        visual.state = .active
        visual.wantsLayer = true
        visual.layer?.cornerRadius = 14
        visual.addSubview(label)
        label.translatesAutoresizingMaskIntoConstraints = false
        NSLayoutConstraint.activate([
            label.centerXAnchor.constraint(equalTo: visual.centerXAnchor),
            label.centerYAnchor.constraint(equalTo: visual.centerYAnchor)
        ])

        let panel = NSPanel(
            contentRect: visual.bounds,
            styleMask: [.borderless, .nonactivatingPanel],
            backing: .buffered,
            defer: false
        )
        panel.contentView = visual
        panel.isOpaque = false
        panel.backgroundColor = .clear
        panel.level = .floating
        panel.hasShadow = true

        if let screen = NSScreen.main {
            panel.setFrameOrigin(CGPoint(
                x: screen.visibleFrame.midX - 80,
                y: screen.visibleFrame.maxY - 96
            ))
        }
        panel.orderFrontRegardless()
        self.panel = panel

        DispatchQueue.main.asyncAfter(deadline: .now() + 1.4) {
            panel.close()
            if self.panel === panel { self.panel = nil }
        }
    }
}
