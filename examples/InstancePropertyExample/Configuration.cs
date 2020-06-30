using System.Collections.Generic;

namespace J4JSoftware.CommandLine.Examples
{
    public class Configuration
    {
        public int IntValue { get; set; }
        public string TextValue { get; set; }
        public List<string> Unkeyed { get; set; }
    }
}