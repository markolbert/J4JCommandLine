using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class RegexTests
    {
        [Theory]
        [InlineData("^-{1,2}|^/|=|:", 3, "-x")]
        [InlineData("^-{1,2}|^/|=|:", 3, "value")]
        [InlineData("^-{1,2}|^/|=|:", 3, "-x:abc")]
        [InlineData("^-{1,2}|^/|=|:", 3, "-x:\"abc\"")]
        public void Splitter( string regex, int maxParts, string input )
        {
            var options = RegexOptions.Compiled | RegexOptions.IgnoreCase;

            var splitter = new Regex(regex, options );

            var parts = splitter.Split( input, maxParts );
        }
    }
}
