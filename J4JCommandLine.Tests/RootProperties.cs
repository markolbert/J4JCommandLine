using System;
using System.Collections.Generic;
using System.Text;

namespace J4JCommandLine.Tests
{
    public class RootProperties
    {
        public string TextProperty { get; set; }
        public int IntProperty { get; set; }
        public bool BoolProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public List<int> IntList { get; set; }
        public int[] IntArray { get; set; }
    }
}
