using System.Collections.Generic;
using System.ComponentModel;
using J4JSoftware.CommandLine;

namespace J4JCommandLine.Tests
{
    public class AutoBindPropertiesBroken
    {
        [OptionKeys("x")]
        [DefaultValue(-1)]
        public int IntProperty { get; set; }

        [OptionKeys("x")]
        [DefaultValue("abc")]
        public string TextProperty { get; set; }

        [OptionKeys()]
        public List<string> Unkeyed1 { get; set; }

        [OptionKeys()]
        public List<string> Unkeyed2 { get; set; }
    }
}