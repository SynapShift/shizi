import AppKit
import CoreGraphics

enum CaptureError: LocalizedError {
    case permissionDenied
    case captureFailed
    case noText

    var errorDescription: String? {
        switch self {
        case .permissionDenied: return "需要屏幕录制权限，请在系统设置中允许拾字后重试。"
        case .captureFailed: return "未能截取所选区域。"
        case .noText: return "所选区域没有识别到文字。"
        }
    }
}

@MainActor
final class CaptureCoordinator {
    private var overlays: [CaptureWindow] = []
    private let preserveLineBreaks: () -> Bool
    private let completion: (Result<String, Error>) -> Void

    init(
        preserveLineBreaks: @escaping () -> Bool,
        completion: @escaping (Result<String, Error>) -> Void
    ) {
        self.preserveLineBreaks = preserveLineBreaks
        self.completion = completion
    }

    func start() {
        guard overlays.isEmpty else { return }

        guard CGPreflightScreenCaptureAccess() || CGRequestScreenCaptureAccess() else {
            completion(.failure(CaptureError.permissionDenied))
            return
        }

        NSApp.hide(nil)
        overlays = NSScreen.screens.map { screen in
            let window = CaptureWindow(screen: screen)
            window.captureView.onCancel = { [weak self] in self?.cancel() }
            window.captureView.onSelection = { [weak self, weak window] rect in
                guard let self, let window else { return }
                let screenRect = window.convertToScreen(rect)
                self.finish(screenRect: screenRect)
            }
            window.orderFrontRegardless()
            return window
        }
        overlays.first?.makeKey()
    }

    private func cancel() {
        closeOverlays()
    }

    private func finish(screenRect: CGRect) {
        let keepLineBreaks = preserveLineBreaks()
        closeOverlays()

        DispatchQueue.main.asyncAfter(deadline: .now() + 0.12) { [weak self] in
            guard let self else { return }
            let mainHeight = NSScreen.screens.first(where: { $0.frame.origin == .zero })?.frame.height
                ?? NSScreen.main?.frame.height
                ?? 0
            let quartzRect = CGRect(
                x: screenRect.minX,
                y: mainHeight - screenRect.maxY,
                width: screenRect.width,
                height: screenRect.height
            )

            guard let image = CGWindowListCreateImage(
                quartzRect,
                .optionOnScreenOnly,
                kCGNullWindowID,
                .bestResolution
            ) else {
                self.completion(.failure(CaptureError.captureFailed))
                return
            }

            Task {
                do {
                    let rawText = try await Task.detached {
                        try VisionOCR.recognize(image)
                    }.value
                    let text = TextCleaner.clean(rawText, preserveLineBreaks: keepLineBreaks)
                    guard !text.isEmpty else { throw CaptureError.noText }
                    NSPasteboard.general.clearContents()
                    NSPasteboard.general.setString(text, forType: .string)
                    ToastPresenter.show("已复制")
                    self.completion(.success(text))
                } catch {
                    ToastPresenter.show("未识别到文字")
                    self.completion(.failure(error))
                }
            }
        }
    }

    private func closeOverlays() {
        overlays.forEach { $0.close() }
        overlays.removeAll()
    }
}

private final class CaptureWindow: NSWindow {
    let captureView = CaptureView()

    init(screen: NSScreen) {
        super.init(
            contentRect: screen.frame,
            styleMask: .borderless,
            backing: .buffered,
            defer: false
        )
        level = .screenSaver
        isOpaque = false
        backgroundColor = .clear
        hasShadow = false
        collectionBehavior = [.canJoinAllSpaces, .fullScreenAuxiliary]
        contentView = captureView
        setFrame(screen.frame, display: true)
    }

    override var canBecomeKey: Bool { true }
}

private final class CaptureView: NSView {
    var onSelection: ((CGRect) -> Void)?
    var onCancel: (() -> Void)?
    private var startPoint: CGPoint?
    private var currentPoint: CGPoint?

    override var acceptsFirstResponder: Bool { true }

    override func viewDidMoveToWindow() {
        super.viewDidMoveToWindow()
        window?.makeFirstResponder(self)
    }

    override func mouseDown(with event: NSEvent) {
        startPoint = convert(event.locationInWindow, from: nil)
        currentPoint = startPoint
        needsDisplay = true
    }

    override func mouseDragged(with event: NSEvent) {
        currentPoint = convert(event.locationInWindow, from: nil)
        needsDisplay = true
    }

    override func mouseUp(with event: NSEvent) {
        currentPoint = convert(event.locationInWindow, from: nil)
        let rect = selectionRect
        if rect.width >= 6, rect.height >= 6 {
            onSelection?(rect)
        } else {
            onCancel?()
        }
    }

    override func keyDown(with event: NSEvent) {
        if event.keyCode == 53 { onCancel?() } else { super.keyDown(with: event) }
    }

    override func draw(_ dirtyRect: NSRect) {
        NSColor.black.withAlphaComponent(0.42).setFill()
        bounds.fill()

        guard startPoint != nil, currentPoint != nil else {
            drawHint()
            return
        }

        let rect = selectionRect
        NSGraphicsContext.current?.cgContext.clear(rect)
        NSColor.white.withAlphaComponent(0.08).setFill()
        rect.fill()
        NSColor(calibratedRed: 0.78, green: 0.96, blue: 0.42, alpha: 1).setStroke()
        let path = NSBezierPath(roundedRect: rect, xRadius: 4, yRadius: 4)
        path.lineWidth = 2
        path.stroke()
    }

    private var selectionRect: CGRect {
        guard let startPoint, let currentPoint else { return .zero }
        return CGRect(
            x: min(startPoint.x, currentPoint.x),
            y: min(startPoint.y, currentPoint.y),
            width: abs(startPoint.x - currentPoint.x),
            height: abs(startPoint.y - currentPoint.y)
        )
    }

    private func drawHint() {
        let text = "拖动框选文字  ·  Esc 取消"
        let attributes: [NSAttributedString.Key: Any] = [
            .font: NSFont.systemFont(ofSize: 14, weight: .medium),
            .foregroundColor: NSColor.white.withAlphaComponent(0.9)
        ]
        let size = text.size(withAttributes: attributes)
        text.draw(
            at: CGPoint(x: (bounds.width - size.width) / 2, y: bounds.height - 72),
            withAttributes: attributes
        )
    }
}
