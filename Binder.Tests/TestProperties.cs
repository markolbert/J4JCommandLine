using System.Collections.Generic;

#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class TestProperties
    {
        public int IntProperty { get; set; }
        public List<int> IntList { get; set; }
        public int[] IntArray { get; set; }
        public List<int> Unkeyed { get; set; }
        public bool Switch { get; set; }
    }
}