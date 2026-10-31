using System.Text.RegularExpressions;

namespace QuadrupleLib.Utilities
{
    public static partial class Regex
    {
        [GeneratedRegex(@"[\\\*\+\?\|\{\[\(\)\^\$\.\#\-\s]")]
        private static partial System.Text.RegularExpressions.Regex EscapeRegex();

        public static string Escape(string s)
        {
            return EscapeRegex().Replace(s, "\\$0");
        }
    }
}
