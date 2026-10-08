using System.Windows;

namespace Shizi.Services;

public sealed class ClipboardService
{
    public async Task<bool> SetTextAsync(string text)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                Clipboard.SetText(text, TextDataFormat.UnicodeText);
                return true;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                await Task.Delay(60 + attempt * 70);
            }
        }

        return false;
    }
}

