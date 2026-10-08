using System.Drawing;

namespace Shizi.Services;

public sealed class FallbackOcrService : IOcrService, IDisposable
{
    private readonly IOcrService _primary;
    private readonly IOcrService _fallback;
    private bool _primaryDisabled;

    public FallbackOcrService(IOcrService primary, IOcrService fallback)
    {
        _primary = primary;
        _fallback = fallback;
    }

    public string Name => _primaryDisabled ? _fallback.Name : _primary.Name;

    public async Task<string> RecognizeAsync(Bitmap source)
    {
        if (!_primaryDisabled)
        {
            try
            {
                var text = await _primary.RecognizeAsync(source);
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException)
            {
                // Native inference or model initialization can fail on unsupported machines.
                _primaryDisabled = true;
            }
        }

        return await _fallback.RecognizeAsync(source);
    }

    public void Dispose()
    {
        if (_primary is IDisposable primaryDisposable) primaryDisposable.Dispose();
        if (_fallback is IDisposable fallbackDisposable) fallbackDisposable.Dispose();
    }
}
