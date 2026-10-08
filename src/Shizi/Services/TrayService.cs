using System.Drawing;
using System.Windows.Forms;

namespace Shizi.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;

    public TrayService(Action showWindow, Action capture, Action exit)
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("开始框选", null, (_, _) => capture());
        menu.Items.Add("打开拾字", null, (_, _) => showWindow());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("退出", null, (_, _) => exit());

        _icon = new NotifyIcon
        {
            Text = "拾字 · Alt + Shift + A",
            Icon = Environment.ProcessPath is { } path
                ? Icon.ExtractAssociatedIcon(path) ?? SystemIcons.Information
                : SystemIcons.Information,
            Visible = true,
            ContextMenuStrip = menu
        };
        _icon.DoubleClick += (_, _) => showWindow();
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
