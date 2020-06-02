using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the configuration information needed by the parser
    public interface IParsingConfiguration
    {
        // the strings used to introduce a key (e.g., "-, --" in "-x --y")
        UniqueText Prefixes { get; }

        // the strings used to enclose parameters in a command line option
        // (e.g., the ':' in "-x:somevalue")
        UniqueText ValueEnclosers { get; }

        // the text delimiters (usually a " or ')
        UniqueText TextDelimiters { get; }

        // the keys which indicate help was requested
        UniqueText HelpKeys { get; }

        // the value describing how key values should be compared for uniqueness (e.g.,
        // whether or not they're case sensitive)
        StringComparison TextComparison { get; }

        // an optional description
        string? Description { get; }

        // an optional program name
        string? ProgramName { get; }
    }
}