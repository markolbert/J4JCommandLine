using System.Collections.Generic;
#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class BasicTarget
    {
        public bool ASwitch { get; set; }
        public string ASingleValue { get; set; }
        public List<string> ACollection { get; set; }
        public TestEnum AnEnumValue { get; set; }
        public TestFlagEnum AFlagEnumValue { get; set; }
    }
}