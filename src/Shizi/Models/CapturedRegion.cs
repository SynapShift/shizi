using System.Drawing;

namespace Shizi.Models;

public sealed record CapturedRegion(Bitmap Bitmap) : IDisposable
{
    public void Dispose() => Bitmap.Dispose();
}

