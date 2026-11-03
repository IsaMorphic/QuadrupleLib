using System.Text;

namespace QuadrupleLib.Utilities
{
    public static class Regex
    {
        private static readonly HashSet<char> _escapeChars = [..@"\*+?|{[()^$.#-"];

        public static string Escape(string s)
        {
            StringBuilder b = new(s.Length * 2); // Allocate enough space to avoid resizing
            for (int i = 0; i < s.Length; i++)
            {
                if (_escapeChars.Contains(s[i]))
                {
                    b.Append('\\');
                }
                b.Append(s[i]);
            }
            return b.ToString();
        }
    }
}
