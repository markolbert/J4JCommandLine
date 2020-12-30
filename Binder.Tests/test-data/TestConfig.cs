using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Configuration.CommandLine;

#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class TestConfig
    {
        public CommandLineStyle Style { get; set; }
        public string CommandLine { get; set; }
        public int UnknownKeys { get; set; }
        public int UnkeyedValues { get; set; }
        public List<OptionConfig> OptionConfigurations { get; set; }
    }
}