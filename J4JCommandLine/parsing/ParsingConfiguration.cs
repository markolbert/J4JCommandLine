using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class ParsingConfiguration : IParsingConfiguration
    {
        public static string[] DefaultPrefixes = { "-", "--", "/" };
        public static string[] DefaultTextDelimiters = { "'", "\"" };
        public static string[] DefaultEnclosers = { "=", ":" };
        public static string[] DefaultHelpKeys = { "h", "?" };

        // an optional description
        public string? Description { get; set; }

        // an optional program name
        public string? ProgramName { get; set; }

        // the strings used to introduce a key (e.g., "-, --" in "-x --y")
        public List<string> Prefixes { get; private set; } = new List<string>();

        // the strings used to enclose parameters in a command line option
        // (e.g., the ':' in "-x:somevalue")
        public List<string> ValueEnclosers { get; private set; } = new List<string>();

        // the text delimiters (usually a " or ')
        public List<string> TextDelimiters { get; private set; } = new List<string>();

        // the keys which indicate help was requested
        public List<string> HelpKeys { get; private set; } = new List<string>();

        // the value describing how key values should be compared for uniqueness (e.g.,
        // whether or not they're case sensitive)
        public StringComparison TextComparison { get; set; } = StringComparison.Ordinal;

        // returns a valid IParsingConfiguration (used to ensure certain properties, like
        // Prefixes, are non-empty)
        public IParsingConfiguration Validate()
        {
            if( Prefixes.Count == 0 )
                Prefixes = new List<string>( DefaultPrefixes );

            if (ValueEnclosers.Count == 0)
                ValueEnclosers = new List<string>(DefaultEnclosers);

            if (TextDelimiters.Count == 0)
                TextDelimiters = new List<string>(DefaultTextDelimiters);

            if (HelpKeys.Count == 0)
                HelpKeys = new List<string>(DefaultHelpKeys);

            return this;
        }
    }
}