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

        private readonly List<string> _prefixes = new List<string>();
        private readonly List<string> _enclosers = new List<string>();
        private readonly List<string> _textDelimiters = new List<string>();
        private readonly List<string> _helpKeys = new List<string>();

        public ParsingConfiguration(
            IJ4JLogger? logger = null
        )
        {
            Logger = logger;

            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public string? Description { get; set; }
        public string? ProgramName { get; set; }

        public List<string> Prefixes => _prefixes.Count == 0 ? new List<string>( DefaultPrefixes ) : _prefixes;
        public List<string> ValueEnclosers => _enclosers.Count == 0 ? new List<string>(DefaultEnclosers) : _enclosers;
        public List<string> TextDelimiters => _textDelimiters.Count == 0 ? new List<string>(DefaultTextDelimiters) : _textDelimiters;
        public List<string> HelpKeys => _helpKeys.Count == 0 ? new List<string>(DefaultHelpKeys) : _helpKeys;

        public StringComparison TextComparison { get; set; } = StringComparison.Ordinal;
    }
}