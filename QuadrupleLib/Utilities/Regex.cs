/*
 *  Copyright 2026 Chosen Few Software
 *  This file is part of QuadrupleLib.
 *
 *  QuadrupleLib is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  QuadrupleLib is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with QuadrupleLib.  If not, see <https://www.gnu.org/licenses/>.
 */

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
                if (char.IsWhiteSpace(s[i]) || 
                    _escapeChars.Contains(s[i]))
                {
                    b.Append('\\');
                }
                b.Append(s[i]);
            }
            return b.ToString();
        }
    }
}
