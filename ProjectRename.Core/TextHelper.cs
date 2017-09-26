using System.Text.RegularExpressions;

namespace ProjectRename.Core
{
    public class TextHelper
    {
        public static string ReplaceText(string text, string search, string replace, string options)
        {
            RegexOptions options1 = RegexOptions.None;
            if (options == null)
            {
                search = search.Replace(".", ".");
                search = search.Replace("?", "?");
                search = search.Replace("*", "*");
                search = search.Replace("(", "(");
                search = search.Replace(")", ")");
                search = search.Replace("[", "[");
                search = search.Replace("[", "[");
                search = search.Replace("[", "[");
                search = search.Replace("{", "{");
                search = search.Replace("}", "}");
                options1 |= RegexOptions.IgnoreCase;
            }
            else if (options.Contains("I"))
            {
                options1 |= RegexOptions.IgnoreCase;
            }

            return Regex.Replace(text, search, replace, options1);
        }
    }
}