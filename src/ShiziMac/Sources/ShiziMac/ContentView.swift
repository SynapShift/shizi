import SwiftUI

struct ContentView: View {
    @ObservedObject var model: AppModel

    private let accent = Color(red: 0.78, green: 0.96, blue: 0.42)
    private let panel = Color(red: 0.09, green: 0.10, blue: 0.095)

    var body: some View {
        ZStack {
            Color(red: 0.045, green: 0.052, blue: 0.048).ignoresSafeArea()

            VStack(spacing: 0) {
                header
                Spacer(minLength: 36)

                HStack(spacing: 44) {
                    hero
                    settings
                }

                Spacer(minLength: 30)
                footer
            }
            .padding(40)
        }
        .preferredColorScheme(.dark)
    }

    private var header: some View {
        HStack(spacing: 10) {
            Image(systemName: "viewfinder")
                .font(.system(size: 20, weight: .semibold))
                .foregroundStyle(accent)
                .frame(width: 32, height: 32)
                .background(panel, in: RoundedRectangle(cornerRadius: 10))
            Text("拾字")
                .font(.system(size: 19, weight: .semibold))
            Spacer()
        }
    }

    private var hero: some View {
        VStack(alignment: .leading, spacing: 0) {
            Text("看见文字，\n顺手拾走。")
                .font(.system(size: 45, weight: .semibold))
                .tracking(-1.2)
                .lineSpacing(4)

            Text("框选网页、图片或桌面上的文字。识别完成后自动复制，直接粘贴到你正在工作的地方。")
                .font(.system(size: 15))
                .foregroundStyle(.secondary)
                .lineSpacing(6)
                .fixedSize(horizontal: false, vertical: true)
                .padding(.top, 22)

            Button(action: model.startCapture) {
                HStack(spacing: 13) {
                    Image(systemName: "viewfinder")
                        .font(.system(size: 18, weight: .semibold))
                        .foregroundStyle(.black)
                        .frame(width: 42, height: 42)
                        .background(accent, in: RoundedRectangle(cornerRadius: 12))

                    VStack(alignment: .leading, spacing: 3) {
                        Text("开始取字").font(.system(size: 15, weight: .semibold))
                        Text("框选屏幕上的任意文字")
                            .font(.system(size: 11))
                            .foregroundStyle(.secondary)
                    }

                    Spacer()
                    Text("⌥ ⇧ A")
                        .font(.system(size: 11, design: .rounded))
                        .foregroundStyle(.secondary)
                        .padding(.horizontal, 9)
                        .padding(.vertical, 6)
                        .background(Color.white.opacity(0.06), in: RoundedRectangle(cornerRadius: 7))
                }
                .padding(15)
                .frame(maxWidth: .infinity)
                .background(panel, in: RoundedRectangle(cornerRadius: 16))
                .overlay(RoundedRectangle(cornerRadius: 16).stroke(Color.white.opacity(0.10)))
            }
            .buttonStyle(.plain)
            .padding(.top, 28)

            if let message = model.message {
                Text(message)
                    .font(.system(size: 12))
                    .foregroundStyle(model.hasError ? Color.red.opacity(0.9) : accent)
                    .padding(.top, 12)
            }
        }
        .frame(maxWidth: 430, alignment: .leading)
    }

    private var settings: some View {
        VStack(alignment: .leading, spacing: 22) {
            VStack(alignment: .leading, spacing: 12) {
                Text("全局快捷键")
                    .font(.system(size: 12))
                    .foregroundStyle(.secondary)
                Text("⌥  +  ⇧  +  A")
                    .font(.system(size: 14, weight: .medium, design: .rounded))
            }

            Divider().overlay(Color.white.opacity(0.08))

            Toggle("保留原始换行", isOn: $model.preserveLineBreaks)
                .toggleStyle(.switch)
            Text("关闭时会自动合并网页和 PDF 中的视觉断行。")
                .font(.system(size: 11))
                .foregroundStyle(.secondary)
                .fixedSize(horizontal: false, vertical: true)

            Divider().overlay(Color.white.opacity(0.08))

            Toggle("登录时自动运行拾字", isOn: Binding(
                get: { model.launchAtLogin },
                set: { model.setLaunchAtLogin($0) }
            ))
            .toggleStyle(.switch)
            Text("登录 macOS 后在菜单栏运行。")
                .font(.system(size: 11))
                .foregroundStyle(.secondary)
        }
        .padding(24)
        .frame(width: 270, alignment: .leading)
        .background(panel, in: RoundedRectangle(cornerRadius: 18))
        .overlay(RoundedRectangle(cornerRadius: 18).stroke(Color.white.opacity(0.09)))
    }

    private var footer: some View {
        HStack {
            Text("本地 OCR · 图片不会上传")
            Spacer()
            Text("关闭窗口后继续在菜单栏运行")
        }
        .font(.system(size: 11))
        .foregroundStyle(Color.secondary.opacity(0.75))
    }
}
