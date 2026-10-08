using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Shizi.Services;

public sealed class GlobalHotkeyService : IDisposable
{
    private const int HotkeyId = 0x5348;
    private const int WmHotkey = 0x0312;
    private const uint ModAlt = 0x0001;
    private const uint ModShift = 0x0004;

    private readonly Window _owner;
    private HwndSource? _source;
    private IntPtr _handle;

    public event EventHandler? Pressed;

    public GlobalHotkeyService(Window owner)
    {
        _owner = owner;
        _owner.SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        _handle = new WindowInteropHelper(_owner).Handle;
        _source = HwndSource.FromHwnd(_handle);
        _source.AddHook(WndProc);

        var key = (uint)KeyInterop.VirtualKeyFromKey(Key.A);
        if (!RegisterHotKey(_handle, HotkeyId, ModAlt | ModShift, key))
        {
            MessageBox.Show(
                "无法注册 Alt + Shift + A，请关闭占用该快捷键的程序后重启拾字。",
                "快捷键冲突",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && wParam.ToInt32() == HotkeyId)
        {
            handled = true;
            Pressed?.Invoke(this, EventArgs.Empty);
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero) UnregisterHotKey(_handle, HotkeyId);
        _source?.RemoveHook(WndProc);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}

