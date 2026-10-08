using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Shizi.Models;
using Point = System.Windows.Point;
using Rectangle = System.Drawing.Rectangle;
using Screen = System.Windows.Forms.Screen;

namespace Shizi.Views;

public partial class CaptureOverlayWindow : Window
{
    private readonly Screen _screen;
    private readonly Bitmap _snapshot;
    private Point _start;
    private bool _dragging;

    public event EventHandler<CapturedRegion>? RegionSelected;
    public event EventHandler? CaptureCancelled;

    public CaptureOverlayWindow(Screen screen, Bitmap snapshot)
    {
        _screen = screen;
        _snapshot = snapshot;
        InitializeComponent();
        ScreenshotImage.Source = ToBitmapSource(snapshot);
        SourceInitialized += PositionOnScreen;
        Closed += (_, _) => _snapshot.Dispose();
        Loaded += (_, _) =>
        {
            UpdateShade(new Rect());
            Focus();
        };
    }

    private void PositionOnScreen(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        SetWindowPos(
            handle,
            HwndTopmost,
            _screen.Bounds.Left,
            _screen.Bounds.Top,
            _screen.Bounds.Width,
            _screen.Bounds.Height,
            SwpShowWindow);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Activate();
        _dragging = true;
        _start = e.GetPosition(Root);
        CaptureMouse();
        Hint.Visibility = Visibility.Collapsed;
        SelectionBorder.Visibility = Visibility.Visible;
        UpdateSelection(_start);
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragging) UpdateSelection(e.GetPosition(Root));
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        ReleaseMouseCapture();

        var rect = Normalize(_start, e.GetPosition(Root));
        if (rect.Width < 8 || rect.Height < 8)
        {
            CaptureCancelled?.Invoke(this, EventArgs.Empty);
            return;
        }

        var scaleX = _snapshot.Width / Math.Max(1, Root.ActualWidth);
        var scaleY = _snapshot.Height / Math.Max(1, Root.ActualHeight);
        var pixelRect = Rectangle.FromLTRB(
            Math.Clamp((int)Math.Round(rect.Left * scaleX), 0, _snapshot.Width - 1),
            Math.Clamp((int)Math.Round(rect.Top * scaleY), 0, _snapshot.Height - 1),
            Math.Clamp((int)Math.Round(rect.Right * scaleX), 1, _snapshot.Width),
            Math.Clamp((int)Math.Round(rect.Bottom * scaleY), 1, _snapshot.Height));

        var cropped = _snapshot.Clone(pixelRect, PixelFormat.Format32bppPArgb);
        RegionSelected?.Invoke(this, new CapturedRegion(cropped));
    }

    private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CaptureCancelled?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateSelection(Point current)
    {
        var rect = Normalize(_start, current);
        Canvas.SetLeft(SelectionBorder, rect.Left);
        Canvas.SetTop(SelectionBorder, rect.Top);
        SelectionBorder.Width = rect.Width;
        SelectionBorder.Height = rect.Height;
        UpdateShade(rect);
    }

    private void UpdateShade(Rect selection)
    {
        var width = Math.Max(0, Root.ActualWidth);
        var height = Math.Max(0, Root.ActualHeight);

        if (selection.IsEmpty || selection.Width <= 0 || selection.Height <= 0)
        {
            PlaceShade(ShadeTop, 0, 0, width, height);
            PlaceShade(ShadeLeft, 0, 0, 0, 0);
            PlaceShade(ShadeRight, 0, 0, 0, 0);
            PlaceShade(ShadeBottom, 0, 0, 0, 0);
            return;
        }

        PlaceShade(ShadeTop, 0, 0, width, selection.Top);
        PlaceShade(ShadeLeft, 0, selection.Top, selection.Left, selection.Height);
        PlaceShade(ShadeRight, selection.Right, selection.Top, Math.Max(0, width - selection.Right), selection.Height);
        PlaceShade(ShadeBottom, 0, selection.Bottom, width, Math.Max(0, height - selection.Bottom));
    }

    private static void PlaceShade(FrameworkElement element, double x, double y, double width, double height)
    {
        Canvas.SetLeft(element, x);
        Canvas.SetTop(element, y);
        element.Width = width;
        element.Height = height;
    }

    private static Rect Normalize(Point first, Point second)
    {
        return new Rect(
            Math.Min(first.X, second.X),
            Math.Min(first.Y, second.Y),
            Math.Abs(first.X - second.X),
            Math.Abs(first.Y - second.Y));
    }

    private static BitmapSource ToBitmapSource(Bitmap bitmap)
    {
        var handle = bitmap.GetHbitmap();
        try
        {
            var source = Imaging.CreateBitmapSourceFromHBitmap(
                handle,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            source.Freeze();
            return source;
        }
        finally
        {
            DeleteObject(handle);
        }
    }

    private static readonly IntPtr HwndTopmost = new(-1);
    private const uint SwpShowWindow = 0x0040;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint flags);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);
}
