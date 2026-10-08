using Shizi.Services;
using System.Diagnostics;
using System.Drawing;

if (args is [var imagePath, ..])
{
    using var input = new Bitmap(imagePath);
    var usePaddle = args.Length > 1 && args[1].Equals("paddle", StringComparison.OrdinalIgnoreCase);
    var timer = Stopwatch.StartNew();
    string text;
    if (usePaddle)
    {
        using var paddle = new PaddleOcrService();
        text = await paddle.RecognizeAsync(input);
    }
    else
    {
        var scale = args.Length > 1 && double.TryParse(args[1], out var requestedScale)
            ? requestedScale
            : (double?)null;
        text = await new WindowsOcrService().RecognizeAsync(input, scale);
    }

    timer.Stop();
    Console.WriteLine(TextCleaner.Clean(text, preserveLineBreaks: false));
    Console.WriteLine($"[{(usePaddle ? "PP-OCRv5" : "Windows OCR")} {timer.ElapsedMilliseconds}ms]");
    return 0;
}

var cases = new[]
{
    new TestCase("中文视觉断行", "网页第一行\n接着的中文", false, "网页第一行接着的中文"),
    new TestCase("英文断行补空格", "Hello\nworld", false, "Hello world"),
    new TestCase("保留段落", "第一段\n\n\n第二段", true, "第一段\n\n第二段"),
    new TestCase("中英混排", "  中英 mixed  \n text  ", false, "中英mixed text"),
    new TestCase(
        "系统 OCR 中文伪空格",
        "而 有 些 地 方 ， 信 息 在 地 下 。 亻 尔 不 知 道 。 标 晋 5 0 0 + ？",
        false,
        "而有些地方，信息在地下。你不知道。标普500+？"),
    new TestCase(
        "细线符号与固定词误识别",
        "全 玉 良 缘 ： 记 住 ， 标 准 只 有 一 个 一 一 攒 够 第 一 个 1 0 0 w 的 年 龄 。",
        false,
        "金玉良缘：记住，标准只有一个——攒够第一个100w的年龄。")
};

var failed = 0;
foreach (var test in cases)
{
    var actual = TextCleaner.Clean(test.Input, test.PreserveLineBreaks);
    if (actual == test.Expected)
    {
        Console.WriteLine($"PASS {test.Name}");
        continue;
    }

    failed++;
    Console.Error.WriteLine($"FAIL {test.Name}\nExpected: [{test.Expected}]\nActual:   [{actual}]");
}

var startupCommand = StartupService.BuildStartupCommand(
    @"C:\Program Files\Shizi\Shizi.exe",
    @"C:\missing-dotnet-runtime");
if (startupCommand == "\"C:\\Program Files\\Shizi\\Shizi.exe\" --startup")
{
    Console.WriteLine("PASS 开机启动命令转义");
}
else
{
    failed++;
    Console.Error.WriteLine($"FAIL 开机启动命令转义\nActual: [{startupCommand}]");
}

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} smoke test(s) failed.");
    return 1;
}

using (var bitmap = new Bitmap(900, 180))
using (var graphics = Graphics.FromImage(bitmap))
using (var font = new Font("Arial", 64, FontStyle.Bold, GraphicsUnit.Pixel))
{
    graphics.Clear(Color.White);
    graphics.DrawString("HELLO 123", font, Brushes.Black, 32, 42);

    var recognized = await new WindowsOcrService().RecognizeAsync(bitmap);
    var normalized = recognized.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    if (normalized.Contains("HELLO123", StringComparison.Ordinal))
    {
        Console.WriteLine($"PASS Windows OCR [{recognized.Trim()}]");
    }
    else
    {
        failed++;
        Console.Error.WriteLine($"FAIL Windows OCR\nActual: [{recognized.Trim()}]");
    }

    using var paddle = new PaddleOcrService();
    var paddleRecognized = await paddle.RecognizeAsync(bitmap);
    var paddleNormalized = paddleRecognized.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
    if (paddleNormalized.Contains("HELLO123", StringComparison.Ordinal))
    {
        Console.WriteLine($"PASS PP-OCRv5 [{paddleRecognized.Trim()}]");
    }
    else
    {
        failed++;
        Console.Error.WriteLine($"FAIL PP-OCRv5\nActual: [{paddleRecognized.Trim()}]");
    }
}

if (failed > 0)
{
    Console.Error.WriteLine($"{failed} smoke test(s) failed.");
    return 1;
}

Console.WriteLine($"All {cases.Length + 3} smoke tests passed.");
return 0;

internal sealed record TestCase(
    string Name,
    string Input,
    bool PreserveLineBreaks,
    string Expected);
