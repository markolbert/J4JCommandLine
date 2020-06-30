using System.Collections.Generic;
using System.ComponentModel;
using J4JSoftware.CommandLine;
#pragma warning disable 8618

namespace J4JCommandLine.Tests
{
    public class AutoBindProperties
    {
        [OptionKeys("i")]
        [DefaultValue(-1)]
        public int IntProperty { get; set; }

        [OptionKeys("t")]
        [DefaultValue("abc")]
        public string TextProperty { get; set; }

        [OptionKeys()]
        public List<string> Unkeyed { get; set; }
    }
}