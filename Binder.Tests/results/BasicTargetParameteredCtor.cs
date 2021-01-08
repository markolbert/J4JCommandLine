using System.Collections.Generic;

namespace J4JSoftware.Binder.Tests
{
    public class BasicTargetParameteredCtor
    {
        private readonly int _value;

        public BasicTargetParameteredCtor( int value )
        {
            _value = value;
        }

        public bool ASwitch { get; set; }
        public string ASingleValue { get; set; }
        public List<string> ACollection { get; set; }
        public TestEnum AnEnumValue { get; set; }
        public TestFlagEnum AFlagEnumValue { get; set; }
    }
}