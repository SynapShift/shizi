import AppKit
import ServiceManagement

@MainActor
final class AppModel: ObservableObject {
    @Published var preserveLineBreaks = false
    @Published private(set) var launchAtLogin = SMAppService.mainApp.status == .enabled
    @Published private(set) var message: String?
    @Published private(set) var hasError = false

    private lazy var captureCoordinator = CaptureCoordinator(
        preserveLineBreaks: { [weak self] in self?.preserveLineBreaks ?? false },
        completion: { [weak self] result in self?.handle(result) }
    )

    func startCapture() {
        message = nil
        hasError = false
        captureCoordinator.start()
    }

    func setLaunchAtLogin(_ enabled: Bool) {
        do {
            if enabled {
                try SMAppService.mainApp.register()
            } else {
                try SMAppService.mainApp.unregister()
            }
            launchAtLogin = enabled
            message = enabled ? "已开启登录时自动运行" : "已关闭登录时自动运行"
            hasError = false
        } catch {
            launchAtLogin = SMAppService.mainApp.status == .enabled
            message = "设置失败：\(error.localizedDescription)"
            hasError = true
        }
    }

    private func handle(_ result: Result<String, Error>) {
        switch result {
        case .success(let text):
            message = "已复制 \(text.count) 个字符"
            hasError = false
        case .failure(let error):
            message = error.localizedDescription
            hasError = true
        }
    }
}
