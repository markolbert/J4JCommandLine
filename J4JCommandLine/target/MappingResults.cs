using System;

namespace J4JSoftware.CommandLine
{
    [Flags]
    public enum MappingResults
    {
        Unbound = 1 << 0,
        UnsupportedMultiplicity = 1 << 1,
        NoKeyFound = 1 << 2,
        ConversionFailed = 1 << 3,
        ValidationFailed = 1 << 4,

        Success = 0
    }
}