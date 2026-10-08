using System.Drawing;

namespace Shizi.Services;

public interface IOcrService
{
    string Name { get; }
    Task<string> RecognizeAsync(Bitmap source);
}

