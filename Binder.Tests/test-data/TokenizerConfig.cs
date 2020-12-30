using System.Collections.Generic;
using J4JSoftware.Configuration.CommandLine;

#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class TokenizerConfig
    {
        public CommandLineStyle Style { get; set; }
        public string CommandLine { get; set; }
        public List<TokenizerData> Data { get; set; }
    }
}