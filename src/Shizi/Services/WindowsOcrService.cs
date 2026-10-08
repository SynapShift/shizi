using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;

namespace Shizi.Services;

public sealed class WindowsOcrService : IOcrService
{
    public string Name => "Windows OCR";

    public Task<string> RecognizeAsync(Bitmap source)
    {
        return RecognizeAsync(source, scaleOverride: null);
    }

    public async Task<string> RecognizeAsync(Bitmap source, double? scaleOverride)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"shizi-{Guid.NewGuid():N}.png");
        Bitmap? resized = null;

        try
        {
            var engine = CreateEngine()
                ?? throw new InvalidOperationException("未找到可用的系统 OCR 语言，请在 Windows 设置中安装中文或英文语言包。");

            var bitmapToSave = source;
            var maxDimension = (int)OcrEngine.MaxImageDimension;
            var scale = scaleOverride ?? ChooseScale(source, maxDimension);
            scale = Math.Min(scale, (double)maxDimension / Math.Max(source.Width, source.Height));
            if (Math.Abs(scale - 1d) > 0.01)
            {
                resized = Resize(source, (int)(source.Width * scale), (int)(source.Height * scale));
                bitmapToSave = resized;
            }

            bitmapToSave.Save(tempPath, ImageFormat.Png);

            var file = await StorageFile.GetFileFromPathAsync(tempPath);
            using var stream = await file.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(stream);
            using var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied);

            var result = await engine.RecognizeAsync(softwareBitmap);
            return string.Join(Environment.NewLine, result.Lines.Select(line => line.Text));
        }
        finally
        {
            resized?.Dispose();
            try
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
            catch
            {
                // A stale temporary image is harmless and can be cleaned by Windows later.
            }
        }
    }

    private static OcrEngine? CreateEngine()
    {
        var preferred = OcrEngine.AvailableRecognizerLanguages.FirstOrDefault(language =>
            language.LanguageTag.StartsWith("zh", StringComparison.OrdinalIgnoreCase));

        preferred ??= OcrEngine.AvailableRecognizerLanguages.FirstOrDefault(language =>
            language.LanguageTag.StartsWith("en", StringComparison.OrdinalIgnoreCase));

        return preferred is not null
            ? OcrEngine.TryCreateFromLanguage(preferred)
            : OcrEngine.TryCreateFromUserProfileLanguages();
    }

    private static Bitmap Resize(Bitmap source, int width, int height)
    {
        var output = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        using var graphics = Graphics.FromImage(output);
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.DrawImage(source, 0, 0, width, height);
        return output;
    }

    private static double ChooseScale(Bitmap source, int maxDimension)
    {
        var largestSide = Math.Max(source.Width, source.Height);
        var maximumAllowedScale = (double)maxDimension / largestSide;

        var preferredScale = source.Height switch
        {
            < 160 => 3d,
            < 320 => 2d,
            _ => 1d
        };

        return Math.Clamp(Math.Min(preferredScale, maximumAllowedScale), 0.1d, 3d);
    }
}
