using System;

namespace J4JSoftware.CommandLine
{
    // a list of all the possible errors that may be encountered during binding and
    // parsing. These are configured as flags so that multiple problems can be reported
    // simultaneously
    [ Flags ]
    public enum MappingResults
    {
        Unbound = 1 << 0,
        UnsupportedMultiplicity = 1 << 1,
        MissingRequired = 1 << 2,
        ConversionFailed = 1 << 3,
        ValidationFailed = 1 << 4,
        TooFewParameters = 1 << 5,
        TooManyParameters = 1 << 6,
        HelpRequested = 1 << 7,
        NotDefinedOrCreatable = 1 << 8,
        NotPublicReadWrite = 1 << 9,

        Success = 0
    }
}