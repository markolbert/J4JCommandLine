using System;

namespace J4JCommandLine.Tests
{
    [Flags]
    public enum FlagsEnum
    {
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 3,

        None = 0,
        All = A | B | C
    }
}