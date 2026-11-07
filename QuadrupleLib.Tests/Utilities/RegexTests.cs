using QuadrupleLib.Utilities;
using Xunit;

namespace QuadrupleLib.Tests.Utilities
{
    public class RegexTests
    {
        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a.c", "a\\.c")]
        [InlineData("a*c", "a\\*c")]
        [InlineData("a+c", "a\\+c")]
        [InlineData("a?c", "a\\?c")]
        [InlineData("a|c", "a\\|c")]
        [InlineData("a{2}c", "a\\{2}c")]
        [InlineData("a[2]c", "a\\[2]c")]
        [InlineData("a(2)c", "a\\(2\\)c")]
        [InlineData("a^2$c", "a\\^2\\$c")]
        [InlineData("a#2-c", "a\\#2\\-c")]
        public void Escape_ShouldEscapeSpecialCharacters(string input, string expected)
        {
            string result = Regex.Escape(input);
            Assert.Equal(expected, result);
        }
    }
}
