using System.Collections.Generic;

namespace InstancePropertyExample
{
    public class Configuration
    {
        public int IntValue { get; set; }
        public string TextValue { get; set; }
        public List<string> Unkeyed { get; set; }
    }
}