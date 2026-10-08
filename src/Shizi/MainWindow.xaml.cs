using System.Windows;
using System.Windows.Input;
using Shizi.Services;

namespace Shizi;

public partial class MainWindow : Window
{
    private readonly StartupService _startupService;
    private bool _isLoadingStartupSetting = true;

    public event EventHandler? CaptureRequested;
    public bool PreserveLineBreaks => PreserveLineBreaksCheckBox.IsChecked == true;
    public bool AllowClose { get; set; }

    public MainWindow(StartupService startupService)
    {
        _startupService = startupService;
        InitializeComponent();
        StartupCheckBox.IsChecked = _startupService.IsEnabled;
        _isLoadingStartupSetting = false;
    }

    private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoadingStartupSetting) return;

        var enabled = StartupCheckBox.IsChecked == true;
        try
        {
            _startupService.SetEnabled(enabled);
            StartupHintText.Text = enabled
                ? "已开启，下次登录 Windows 后会在托盘静默运行。"
                : "已关闭，不会随 Windows 自动运行。";
            StartupHintText.Foreground = (System.Windows.Media.Brush)FindResource("MutedBrush");
        }
        catch (Exception exception)
        {
            _isLoadingStartupSetting = true;
            StartupCheckBox.IsChecked = !enabled;
            _isLoadingStartupSetting = false;
            StartupHintText.Text = $"设置失败：{exception.Message}";
            StartupHintText.Foreground = System.Windows.Media.Brushes.IndianRed;
        }
    }

    private void CaptureButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        CaptureRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;

        if (e.ClickCount == 2)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            return;
        }

        DragMove();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
