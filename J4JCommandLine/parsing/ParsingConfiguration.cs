using System;

namespace J4JSoftware.CommandLine
{
    public class ParsingConfiguration : IParsingConfiguration
    {
        public static string[] DefaultPrefixes = { "-", "--", "/" };
        public static string[] DefaultTextDelimiters = { "'", "\"" };
        public static string[] DefaultEnclosers = { "=", ":" };
        public static string[] DefaultHelpKeys = { "h", "?" };

        private UniqueText? _prefixes = null;
        private UniqueText? _enclosers = null;
        private UniqueText? _delimiters = null;
        private UniqueText? _helpKeys = null;
        private bool _textCompChanged = false;

        // an optional description
        public string? Description { get; set; }

        // an optional program name
        public string? ProgramName { get; set; }

        // the strings used to introduce a key (e.g., "-, --" in "-x --y")
        public UniqueText Prefixes
        {
            get
            {
                if( _textCompChanged || _prefixes == null )
                {
                    var newCollection = new UniqueText(TextComparison);

                    if( _prefixes == null )
                        newCollection.AddRange( DefaultPrefixes );
                    else newCollection.AddRange( _prefixes );

                    _prefixes = newCollection;
                }

                return _prefixes;
            }
        }

        // the strings used to enclose parameters in a command line option
        // (e.g., the ':' in "-x:somevalue")
        public UniqueText ValueEnclosers
        {
            get
            {
                if (_textCompChanged || _enclosers == null)
                {
                    var newCollection = new UniqueText(TextComparison);

                    if (_enclosers == null)
                        newCollection.AddRange(DefaultEnclosers);
                    else newCollection.AddRange(_enclosers);

                    _enclosers = newCollection;
                }

                return _enclosers;
            }
        }

        // the text delimiters (usually a " or ')
        public UniqueText TextDelimiters
        {
            get
            {
                if (_textCompChanged || _delimiters == null)
                {
                    var newCollection = new UniqueText(TextComparison);

                    if (_delimiters== null)
                        newCollection.AddRange(DefaultTextDelimiters);
                    else newCollection.AddRange(_delimiters);

                    _delimiters = newCollection;
                }

                return _delimiters;
            }
        }

        // the keys which indicate help was requested
        public UniqueText HelpKeys
        {
            get
            {
                if( _textCompChanged || _helpKeys == null )
                {
                    var newCollection = new UniqueText( TextComparison );

                    if( _helpKeys == null )
                        newCollection.AddRange( DefaultHelpKeys );
                    else newCollection.AddRange( _helpKeys );

                    _helpKeys = newCollection;
                }

                return _helpKeys;
            }
        }

        // the value describing how key values should be compared for uniqueness (e.g.,
        // whether or not they're case sensitive)
        public StringComparison TextComparison { get; set; } = StringComparison.Ordinal;
    }
}