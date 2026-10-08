using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Shizi.Views;

public enum ToastKind
{
    Success,
    Progress,
    Error
}

public partial class ToastWindow : Window
{
    private static ToastWindow? _current;

    public ToastWindow(string title, string message, ToastKind kind, int durationMs)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;

        switch (kind)
        {
            case ToastKind.Progress:
                Glyph.Text = "···";
                GlyphBackground.Background = new SolidColorBrush(Color.FromRgb(110, 207, 255));
                break;
            case ToastKind.Error:
                Glyph.Text = "!";
                GlyphBackground.Background = new SolidColorBrush(Color.FromRgb(255, 126, 112));
                break;
        }

        Loaded += (_, _) =>
        {
            var area = SystemParameters.WorkArea;
            Left = area.Right - ActualWidth - 22;
            Top = area.Bottom - ActualHeight - 22;

            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150)));

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                var animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
                animation.Completed += (_, _) => Close();
                BeginAnimation(OpacityProperty, animation);
            };
            timer.Start();
        };

        Closed += (_, _) =>
        {
            if (ReferenceEquals(_current, this)) _current = null;
        };
    }

    public static void ShowMessage(
        string title,
        string message,
        ToastKind kind,
        int durationMs = 2400)
    {
        _current?.Close();
        _current = new ToastWindow(title, message, kind, durationMs);
        _current.Show();
    }
}
