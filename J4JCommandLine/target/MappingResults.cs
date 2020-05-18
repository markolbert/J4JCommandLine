using System;

namespace J4JSoftware.CommandLine
{
    [Flags]
    public enum MappingResults
    {
        Unbound = 1 << 0,
        UnsupportedMultiplicity = 1 << 1,
        MissingRequired = 1 << 2,
        ConversionFailed = 1 << 3,
        ValidationFailed = 1 << 4,
        TooFewParameters = 1 << 5,
        TooManyParameters = 1 << 6,

        Success = 0
    }
}