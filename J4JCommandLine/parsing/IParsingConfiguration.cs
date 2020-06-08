using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the configuration information needed by the parser
    public interface IParsingConfiguration
    {
        // the strings used to introduce a key (e.g., "-, --" in "-x --y")
        ImmutableTextCollection Prefixes { get; }

        // the strings used to enclose parameters in a command line option
        // (e.g., the ':' in "-x:somevalue")
        ImmutableTextCollection ValueEnclosers { get; }

        // the text delimiters (usually a " or ')
        ImmutableTextCollection TextDelimiters { get; }

        // the keys which indicate help was requested
        ImmutableTextCollection HelpKeys { get; }

        // the value describing how key values should be compared for uniqueness (e.g.,
        // whether or not they're case sensitive)
        StringComparison TextComparison { get; }

        // an optional description
        string? Description { get; }

        // an optional program name
        string? ProgramName { get; }
    }
}