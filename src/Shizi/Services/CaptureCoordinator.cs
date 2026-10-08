using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Shizi.Models;
using Shizi.Views;

namespace Shizi.Services;

public sealed class CaptureCoordinator
{
    private readonly IOcrService _ocr;
    private readonly ClipboardService _clipboard;
    private readonly Func<bool> _preserveLineBreaks;
    private readonly List<CaptureOverlayWindow> _overlays = [];
    private bool _isCapturing;

    public CaptureCoordinator(
        IOcrService ocr,
        ClipboardService clipboard,
        Func<bool> preserveLineBreaks)
    {
        _ocr = ocr;
        _clipboard = clipboard;
        _preserveLineBreaks = preserveLineBreaks;
    }

    public async Task StartAsync()
    {
        if (_isCapturing) return;
        _isCapturing = true;

        try
        {
            await Task.Delay(90);
            foreach (var screen in Screen.AllScreens)
            {
                var snapshot = CaptureScreen(screen);
                var overlay = new CaptureOverlayWindow(screen, snapshot);
                overlay.RegionSelected += OnRegionSelected;
                overlay.CaptureCancelled += OnCaptureCancelled;
                _overlays.Add(overlay);
            }

            foreach (var overlay in _overlays) overlay.Show();
        }
        catch (Exception ex)
        {
            CloseOverlays();
            _isCapturing = false;
            ToastWindow.ShowMessage("无法开始框选", ex.Message, ToastKind.Error);
        }
    }

    private async void OnRegionSelected(object? sender, CapturedRegion region)
    {
        CloseOverlays();

        try
        {
            ToastWindow.ShowMessage("正在识别", $"{_ocr.Name} · 图片只在本机处理", ToastKind.Progress, 8000);
            using (region)
            {
                var rawText = await _ocr.RecognizeAsync(region.Bitmap);
                var text = TextCleaner.Clean(rawText, _preserveLineBreaks());

                if (string.IsNullOrWhiteSpace(text))
                {
                    ToastWindow.ShowMessage("没有识别到文字", "换一个更清晰的区域再试试", ToastKind.Error);
                    return;
                }

                if (!await _clipboard.SetTextAsync(text))
                {
                    ToastWindow.ShowMessage("复制失败", "剪贴板正被其他程序占用，请重试", ToastKind.Error);
                    return;
                }

                ToastWindow.ShowMessage("已复制", $"{text.Length} 个字符 · 现在可以直接粘贴", ToastKind.Success);
            }
        }
        catch (Exception ex)
        {
            ToastWindow.ShowMessage("识别失败", ex.Message, ToastKind.Error, 4200);
        }
        finally
        {
            _isCapturing = false;
        }
    }

    private void OnCaptureCancelled(object? sender, EventArgs e)
    {
        CloseOverlays();
        _isCapturing = false;
    }

    private void CloseOverlays()
    {
        foreach (var overlay in _overlays.ToArray())
        {
            overlay.RegionSelected -= OnRegionSelected;
            overlay.CaptureCancelled -= OnCaptureCancelled;
            overlay.Close();
        }

        _overlays.Clear();
    }

    private static Bitmap CaptureScreen(Screen screen)
    {
        var bounds = screen.Bounds;
        var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
        return bitmap;
    }
}
