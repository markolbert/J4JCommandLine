﻿using System.Collections.Generic;

namespace J4JCommandLine.Tests
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