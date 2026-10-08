using System.Text.RegularExpressions;

namespace Shizi.Services;

public static class TextCleaner
{
    private const string CjkCharacters = "\\u2E80-\\u2FFF\\u3000-\\u303F\\u31C0-\\u31EF\\u3400-\\u4DBF\\u4E00-\\u9FFF\\uF900-\\uFAFF\\uFF00-\\uFFEF";
    private static readonly Regex HorizontalWhitespaceRegex = new("[\\t\\f\\v ]+");
    private static readonly Regex ExcessBlankLinesRegex = new("\\n{3,}");
    private static readonly Regex CjkBoundaryWhitespaceRegex = new(
        $"(?<=[{CjkCharacters}]) +(?=\\S)|(?<=\\S) +(?=[{CjkCharacters}])");
    private static readonly Regex DigitWhitespaceRegex = new("(?<=\\d) +(?=\\d)");
    private static readonly Regex NumberSingleLetterUnitWhitespaceRegex = new(
        $"(?<=\\d) +(?=[a-zA-Z](?:[{CjkCharacters}]|$))");
    private static readonly Regex BeforePunctuationWhitespaceRegex = new(" +(?=[,.;:!?%+，。！？；：、）》】])");
    private static readonly Regex AfterOpeningPunctuationWhitespaceRegex = new("(?<=[(（《【]) +");
    private static readonly Regex Sp500CorrectionRegex = new("标晋(?=\\d{3})");
    private static readonly Regex QuantifierDashCorrectionRegex = new(
        $"(一个|一句|一段|一种|一件|一次|一点)(?:一一|—)(?=[{CjkCharacters}])");

    public static string Clean(string text, bool preserveLineBreaks)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        normalized = HorizontalWhitespaceRegex.Replace(normalized, " ");
        normalized = CjkBoundaryWhitespaceRegex.Replace(normalized, string.Empty);
        normalized = DigitWhitespaceRegex.Replace(normalized, string.Empty);
        normalized = NumberSingleLetterUnitWhitespaceRegex.Replace(normalized, string.Empty);
        normalized = BeforePunctuationWhitespaceRegex.Replace(normalized, string.Empty);
        normalized = AfterOpeningPunctuationWhitespaceRegex.Replace(normalized, string.Empty);
        normalized = ApplyHighConfidenceOcrCorrections(normalized);

        if (preserveLineBreaks)
        {
            normalized = ExcessBlankLinesRegex.Replace(normalized, "\n\n");
            return normalized.Trim();
        }

        var lines = normalized.Split('\n')
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();

        if (lines.Length == 0) return string.Empty;

        var result = lines[0];
        for (var i = 1; i < lines.Length; i++)
        {
            var needsSpace = NeedsSpace(result[^1], lines[i][0]);
            result += needsSpace ? " " + lines[i] : lines[i];
        }

        return result.Trim();
    }

    private static bool NeedsSpace(char left, char right)
    {
        return IsLatinLike(left) && IsLatinLike(right);
    }

    private static bool IsLatinLike(char value)
    {
        return char.IsLetterOrDigit(value) && value <= 0x024F;
    }

    private static string ApplyHighConfidenceOcrCorrections(string text)
    {
        text = text.Replace("亻尔", "你", StringComparison.Ordinal);
        text = text.Replace("全玉良缘", "金玉良缘", StringComparison.Ordinal);
        text = Sp500CorrectionRegex.Replace(text, "标普");
        return QuantifierDashCorrectionRegex.Replace(text, "$1——");
    }
}
