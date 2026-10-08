using Microsoft.Win32;

namespace Shizi.Services;

public sealed class StartupService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Shizi";

    public bool IsEnabled
    {
        get
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return key?.GetValue(ValueName) is string command && !string.IsNullOrWhiteSpace(command);
        }
    }

    public void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("无法打开 Windows 启动项设置。");

        if (enabled)
        {
            key.SetValue(ValueName, BuildStartupCommand(), RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
        }
    }

    public static string BuildStartupCommand(string? executablePath = null, string? dotnetRoot = null)
    {
        var executable = executablePath ?? Environment.ProcessPath
            ?? throw new InvalidOperationException("无法确定拾字的程序路径。");

        dotnetRoot ??= Environment.GetEnvironmentVariable("DOTNET_ROOT");
        var dotnet = string.IsNullOrWhiteSpace(dotnetRoot) ? null : Path.Combine(dotnetRoot, "dotnet.exe");
        var managedDll = Path.ChangeExtension(executable, ".dll");

        // 开发版可能依赖仓库内的 .NET 运行时；正式自包含版本则直接启动 exe。
        if (dotnet is not null && File.Exists(dotnet) && File.Exists(managedDll))
        {
            return $"\"{dotnet}\" \"{managedDll}\" --startup";
        }

        return $"\"{executable}\" --startup";
    }
}
