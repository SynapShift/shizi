using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace Shizi.Services;

public sealed class PaddleOcrService : IOcrService, IDisposable
{
    private readonly Lazy<PaddleOcrAll> _engine = new(CreateEngine);
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public string Name => "PP-OCRv5";

    public async Task<string> RecognizeAsync(Bitmap source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte[] imageBytes;
        using (var stream = new MemoryStream())
        {
            source.Save(stream, ImageFormat.Png);
            imageBytes = stream.ToArray();
        }

        await _gate.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                using var image = Cv2.ImDecode(imageBytes, ImreadModes.Color);
                if (image.Empty()) return string.Empty;

                var result = _engine.Value.Run(image);
                return result.Text;
            });
        }
        finally
        {
            _gate.Release();
        }
    }

    private static PaddleOcrAll CreateEngine()
    {
        var engine = new PaddleOcrAll(LocalFullModels.ChineseV5, PaddleDevice.OneDnn())
        {
            AllowRotateDetection = false,
            Enable180Classification = false
        };

        engine.Detector.MaxSize = 2048;

        return engine;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_engine.IsValueCreated) _engine.Value.Dispose();
        _gate.Dispose();
    }
}
