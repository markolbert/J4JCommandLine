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
        private readonly List<string> _delimiters = new List<string>();
        private readonly List<string> _enclosers = new List<string>();

        private readonly List<string> _prefixes = new List<string>();

        public ParsingConfiguration(
            IJ4JLogger? logger = null
        )
        {
            Logger = logger;

            _prefixes.AddRange( DefaultPrefixes );
            _enclosers.AddRange( DefaultEnclosers );
            _delimiters.AddRange( DefaultTextDelimiters );

            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public ReadOnlyCollection<string> Prefixes => _prefixes.AsReadOnly();
        public ReadOnlyCollection<string> ValueEnclosers => _enclosers.AsReadOnly();
        public ReadOnlyCollection<string> TextDelimiters => _delimiters.AsReadOnly();

        public StringComparison TextComparison { get; set; } = StringComparison.Ordinal;

        public void AddPrefixes( params string[] toAdd )
        {
            AddText( _prefixes, toAdd, "prefixes" );
        }

        public void AddValueEnclosers( params string[] toAdd )
        {
            AddText( _enclosers, toAdd, "value enclosers" );
        }

        public void AddTextDelimiters( params string[] toAdd )
        {
            AddText( _delimiters, toAdd, "text delimiters" );
        }

        protected void AddText( List<string> list, string[] toAdd, string listName )
        {
            if( toAdd == null || toAdd.Length == 0 )
            {
                Logger?.Warning( "Nothing to add" );
                return;
            }

            foreach( var text in toAdd )
                if( !list.Any( x => string.Equals( text, x, TextComparison ) ) )
                {
                    list.Add( text );

                    Logger?.Verbose<string, string>( "Added {text} as {listName} text", text, listName );
                }
                else
                {
                    Logger?.Warning<string, string>( "{text} already included as {listName} text", text, listName );
                }
        }
    }
}