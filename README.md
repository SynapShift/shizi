<p align="center">
  <img src="src/Shizi/Assets/Shizi.png" width="72" alt="拾字图标" />
</p>

<h1 align="center">拾字 Shizi</h1>

> 看见文字，顺手拾走。

拾字是一个本地优先的 Windows 屏幕取字工具。按下全局快捷键，框选网页、图片、扫描 PDF 或桌面上的文字；识别完成后内容会自动进入剪贴板，可以直接粘贴到正在工作的地方。

项目目前处于早期开发阶段，欢迎试用、报告问题和参与贡献。

## 当前能力

- `Alt + Shift + A` 全局快捷键唤起
- 多显示器区域框选
- PP-OCRv5 中文模型优先、Windows OCR 自动兜底的离线识别
- 自动合并网页和 PDF 的视觉断行
- 自动复制到剪贴板
- 托盘常驻与轻量结果提示
- 可选保留原始换行
- 可在主界面设置是否随 Windows 开机自动运行

## 设计原则

- **少一步：** 默认流程只有唤起、框选、粘贴。
- **本地优先：** 当前版本不会上传截图。
- **安静但精致：** 不弹出阻塞式结果窗口，反馈短促清晰。
- **能力克制：** 新功能必须围绕“框选后的下一步”，不做杂乱的工具箱。

## 环境要求

- Windows 10 版本 1903 或更高版本，推荐 Windows 11
- .NET 8 SDK（开发与构建）
- 建议安装中文或英文 Windows 语言包，以便高精度引擎不可用时使用系统 OCR 兜底

## 本地运行

已经构建过项目时，可以双击 `run.cmd`。也可以在终端运行：

```powershell
dotnet run --project .\src\Shizi\Shizi.csproj
```

构建 Release：

```powershell
dotnet build .\src\Shizi\Shizi.csproj -c Release
```

运行 smoke tests：

```powershell
dotnet run --project .\tests\Shizi.SmokeTests\Shizi.SmokeTests.csproj -c Release
```

## 使用方法

1. 启动拾字。
2. 按 `Alt + Shift + A`，或点击首页的“开始框选”。
3. 拖动鼠标框住需要提取的文字。
4. 松开鼠标，等待“已复制”提示。
5. 在任意输入框按 `Ctrl + V`。

按 `Esc` 可以取消框选。关闭主窗口后，拾字会继续在系统托盘运行。
“开机时自动运行拾字”可在主界面直接开启或关闭；自动启动时不会弹出主窗口，只在托盘静默运行。

## 路线图

- [ ] 可配置全局快捷键
- [x] PP-OCRv5 本地中文 OCR 引擎
- [ ] 翻译后复制
- [ ] 图片表格转可编辑表格
- [ ] 可选的本地识别历史
- [ ] Chromium 浏览器扩展：DOM 精确提取、OCR 兜底
- [ ] 正式应用图标、安装包和自动 Release

## 隐私

当前实现优先使用本地 PP-OCRv5 模型，失败时自动切换到 Windows OCR，截图不会发送到网络。Windows OCR 兜底过程中会在系统临时目录创建短生命周期的 PNG 文件，并在识别结束后立即删除。

若未来加入云端 OCR 或 AI 能力，相关功能必须明确标注、默认关闭，并提供完全本地运行的路径。

## 贡献

项目尚处于快速成形阶段。提交 Issue 时请尽量附上：

- Windows 版本和显示缩放比例
- 单屏或多屏环境
- 可脱敏的原始截图
- 期望文字与实际识别结果

## License

[MIT](LICENSE)
