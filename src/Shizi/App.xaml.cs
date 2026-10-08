using System.Windows;
using Shizi.Services;

namespace Shizi;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private GlobalHotkeyService? _hotkey;
    private TrayService? _tray;
    private CaptureCoordinator? _capture;
    private FallbackOcrService? _ocr;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var startupService = new StartupService();
        _mainWindow = new MainWindow(startupService);
        _ocr = new FallbackOcrService(
            new PaddleOcrService(),
            new WindowsOcrService());
        _capture = new CaptureCoordinator(
            _ocr,
            new ClipboardService(),
            () => _mainWindow.PreserveLineBreaks);

        _hotkey = new GlobalHotkeyService(_mainWindow);
        _hotkey.Pressed += (_, _) =>
        {
            _mainWindow.Hide();
            _ = _capture.StartAsync();
        };
        _mainWindow.CaptureRequested += (_, _) => _ = _capture.StartAsync();

        _tray = new TrayService(
            showWindow: ShowMainWindow,
            capture: () => _ = _capture.StartAsync(),
            exit: ExitApplication);

        _mainWindow.Closing += (_, args) =>
        {
            if (!_mainWindow.AllowClose)
            {
                args.Cancel = true;
                _mainWindow.Hide();
            }
        };

        if (!e.Args.Contains("--startup", StringComparer.OrdinalIgnoreCase))
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        if (_mainWindow is not null) _mainWindow.AllowClose = true;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_mainWindow is not null) _mainWindow.AllowClose = true;
        _tray?.Dispose();
        _hotkey?.Dispose();
        _ocr?.Dispose();
        base.OnExit(e);
    }
}
