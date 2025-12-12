using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
internal static class FilterBotAnswer
{
    private static String EscapeMarkdownV2(String text)
    {
        var specialChars = new[] { "_", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
        foreach (var ch in specialChars)
        {
            text = text.Replace(ch, "\\" + ch);
        }
        return text;
    }


    //internal static String RemoveBracketsLinks(String input)
    //{
    //    String pattern = @"\[\d+\]";
    //    return Regex.Replace(input, pattern, "");
    //}

    internal static String ConvertText(String input)
    {
        String pattern = @"\*\*(.+?)\*\*";
        String replaced = Regex.Replace(input, pattern, "*$1*");
        replaced = EscapeMarkdownV2(replaced);
        return replaced;
    }

}
