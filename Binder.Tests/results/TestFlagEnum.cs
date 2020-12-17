using System;

namespace J4JSoftware.Binder.Tests
{
    [Flags]
    public enum TestFlagEnum
    {
        EnumValue1 = 1 << 0,
        EnumValue2 = 1 << 1,
    }
}