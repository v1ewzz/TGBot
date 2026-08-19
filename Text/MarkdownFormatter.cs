using System.Text.RegularExpressions;

namespace TGBot.Text;

public static class MarkdownFormatter
{
    private static readonly char[] SpecialChars =
        ['_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!'];

    private static readonly Regex BoldRegex = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);

    public static string ToTelegram(string input)
    {
        string escaped = EscapeMarkdownV2(input);
        return BoldRegex.Replace(escaped, "*$1*");
    }

    private static string EscapeMarkdownV2(string text)
    {
        text = text.Replace("\\", "\\\\");
        return SpecialChars.Aggregate(text, (current, ch) => current.Replace(ch.ToString(), "\\" + ch));
    }
}