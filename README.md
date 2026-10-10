<p align="center">
  <img src="src/Shizi/Assets/Shizi.png" width="88" alt="Shizi logo" />
</p>

<h1 align="center">Shizi — Screen OCR to Clipboard for Windows</h1>

<p align="center">Capture any text on your screen, recognize it locally, and paste it anywhere.</p>

<p align="center"><strong>English</strong> · <a href="README.zh-CN.md">简体中文</a></p>

Shizi (拾字) is an open-source, privacy-first **Windows screen OCR**, **screenshot text extractor**, and **OCR-to-clipboard** utility powered by PP-OCRv5. Press a global shortcut, draw a box around text in a webpage, image, scanned PDF, video, or desktop app, and the recognized text is copied to your clipboard automatically.

## Download

Download the latest version from [GitHub Releases](https://github.com/SynapShift/shizi/releases/latest).

| Package | Best for |
| --- | --- |
| `Shizi-Setup-*-win-x64.exe` | Recommended. Install once, then launch Shizi from the Start menu. |
| `Shizi-*-win-x64.zip` | Portable. Extract the archive and double-click `Shizi.exe`. |

Both packages include the .NET runtime and local OCR models. No separate runtime or model download is required.

**System requirements:** Windows 10 version 1903 or later, x64. Windows 11 is recommended.

## Features

- Global `Alt + Shift + A` capture shortcut
- Multi-monitor region capture
- Local PP-OCRv5 Chinese and English recognition
- Automatic Windows OCR fallback
- Smart cleanup of visual line breaks and artificial Chinese spacing
- Instant copy to clipboard
- Optional preservation of original line breaks
- System tray operation and lightweight result feedback
- User-controlled launch at Windows sign-in
- No screenshot uploads

## Usage

1. Start Shizi.
2. Press `Alt + Shift + A`, or choose **Start capture** in the main window.
3. Drag around the text you want to extract.
4. Release the mouse and wait for the copied notification.
5. Press `Ctrl + V` in any text field.

Press `Esc` to cancel a capture without changing the clipboard. Closing the main window keeps Shizi running in the system tray.

## Why Shizi?

- **One less step:** capture, select, paste—no result dialog to manage.
- **Local by default:** OCR runs on your device and screenshots are not uploaded.
- **Built for Chinese text:** PP-OCRv5 plus cleanup rules handle mixed Chinese and English content.
- **Works beyond the browser:** extract text from desktop apps, images, videos, and scanned documents.

## Build from source

Development requires the .NET 8 SDK on Windows.

```powershell
dotnet run --project .\src\Shizi\Shizi.csproj
dotnet build .\src\Shizi\Shizi.csproj -c Release
dotnet run --project .\tests\Shizi.SmokeTests\Shizi.SmokeTests.csproj -c Release
```

## Roadmap

- [ ] Configurable global shortcut
- [x] Local PP-OCRv5 engine
- [x] Installer and portable Windows packages
- [ ] Translate and copy
- [ ] Extract image tables into editable data
- [ ] Optional local recognition history
- [ ] Chromium extension with DOM extraction and OCR fallback

## Privacy

Shizi uses the bundled local PP-OCRv5 model first and falls back to the Windows OCR API. Screenshots are never sent to a server. The Windows OCR fallback may create a short-lived PNG in the system temporary directory; it is deleted immediately after recognition.

## Feedback and contributions

Issues and pull requests are welcome. Use the [issue form](https://github.com/SynapShift/shizi/issues/new/choose) for bug reports. Remove account details, conversations, and other sensitive information before attaching screenshots.

## License

[MIT](LICENSE)
