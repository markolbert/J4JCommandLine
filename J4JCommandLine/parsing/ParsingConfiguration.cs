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

        public List<string> Prefixes => _prefixes.Count == 0 ? new List<string>( DefaultPrefixes ) : _prefixes;
        public List<string> ValueEnclosers => _enclosers.Count == 0 ? new List<string>(DefaultEnclosers) : _enclosers;
        public List<string> TextDelimiters => _textDelimiters.Count == 0 ? new List<string>(DefaultTextDelimiters) : _textDelimiters;
        public List<string> HelpKeys => _helpKeys.Count == 0 ? new List<string>(DefaultHelpKeys) : _helpKeys;

        public StringComparison TextComparison { get; set; } = StringComparison.Ordinal;

        //public void AddPrefixes( params string[] toAdd )
        //{
        //    AddText( _prefixes, toAdd, "prefixes" );
        //}

        //public void AddValueEnclosers( params string[] toAdd )
        //{
        //    AddText( _enclosers, toAdd, "value enclosers" );
        //}

        //public void AddTextDelimiters( params string[] toAdd )
        //{
        //    AddText( _delimiters, toAdd, "text delimiters" );
        //}

        //public void AddTextDelimiters(params string[] toAdd)
        //{
        //    AddText(_delimiters, toAdd, "text delimiters");
        //}

        //protected void AddText( List<string> list, string[] toAdd, string listName )
        //{
        //    if( toAdd == null || toAdd.Length == 0 )
        //    {
        //        Logger?.Warning( "Nothing to add" );
        //        return;
        //    }

        //    foreach( var text in toAdd )
        //        if( !list.Any( x => string.Equals( text, x, TextComparison ) ) )
        //        {
        //            list.Add( text );

        //            Logger?.Verbose<string, string>( "Added {text} as {listName} text", text, listName );
        //        }
        //        else
        //        {
        //            Logger?.Warning<string, string>( "{text} already included as {listName} text", text, listName );
        //        }
        //}
    }
}