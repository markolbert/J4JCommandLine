using System.Collections.Generic;
#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class TestConfig
    {
        public string CommandLine { get; set; }
        public int UnknownKeys { get; set; }
        public int UnkeyedParameters { get; set; }
        public List<OptionConfig> OptionConfigurations { get; set; }
    }
}