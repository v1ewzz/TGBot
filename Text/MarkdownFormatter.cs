using System.Text;
using System.Text.RegularExpressions;

namespace TGBot.Text;

public static class MarkdownFormatter
{
    private static readonly Regex BoldRegex = new(@"\*\*(.+?)\*\*", RegexOptions.Compiled);
    private static readonly Regex HeadingRegex = new(@"^\s{0,3}#{1,6}\s+(.+)$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex UnorderedListRegex = new(@"^\s*[-*]\s+", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex HorizontalRuleRegex = new(@"^\s*(---+|\*\*\*+)\s*$", RegexOptions.Compiled | RegexOptions.Multiline);

    public static string ToTelegram(string input)
    {
        string html = EscapeHtml(input);
        html = BoldRegex.Replace(html, "<b>$1</b>");
        html = HeadingRegex.Replace(html, "<b>$1</b>\n");
        html = HorizontalRuleRegex.Replace(html, "");
        html = UnorderedListRegex.Replace(html, "• ");
        return html.Trim();
    }

    private static string EscapeHtml(string text)
    {
        var sb = new StringBuilder(text.Length);
        foreach (char ch in text)
        {
            sb.Append(ch switch
            {
                '&' => "&amp;",
                '<' => "&lt;",
                '>' => "&gt;",
                _ => ch.ToString()
            });
        }
        return sb.ToString();
    }
}