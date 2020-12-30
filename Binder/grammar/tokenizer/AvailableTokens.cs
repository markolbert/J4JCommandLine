using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class AvailableTokens
    {
        private readonly Dictionary<TokenType, List<string>> _available = new();
        private readonly IJ4JLogger? _logger;

        public AvailableTokens( StringComparison textComp, IJ4JLogger? logger )
        {
            TextComparison = textComp;
            _logger = logger;
        }

        public StringComparison TextComparison { get; }

        public IEnumerable<(string text, TokenType type)> Available
        {
            get
            {
                foreach( var kvp in _available )
                foreach( var itemText in kvp.Value )
                    yield return ( itemText, kvp.Key );
            }
        }

        public int Count => _available.Count;

        public bool Add( TokenType type, string text )
        {
            if( type == TokenType.Text || type == TokenType.StartOfInput )
            {
                _logger?.Error( "Cannot include {0} tokens", type );
                return false;
            }

            if( _available.SelectMany( kvp => kvp.Value )
                .Any( t => t.Equals( text, TextComparison ) ) )
            {
                _logger?.Error( "Duplicate token text '{0}' ({1})", text, type);
                return false;
            }

            if( _available.ContainsKey( type ) )
                _available[ type ].Add( text );
            else _available.Add( type, new List<string> { text } );

            return true;
        }

        public bool Remove( string text )
        {
            var kvp = _available.FirstOrDefault(
                x => x.Value.Any( t => t.Equals( text, TextComparison ) ) );

            var idx = kvp.Value.FindIndex( x => x.Equals( text, TextComparison ) );

            if( idx < 0 )
            {
                _logger?.Error<string>( "Couldn't find '{0}' among tokens to delete", text );
                return false;
            }

            kvp.Value.RemoveAt( idx );

            return true;
        }

        public void Clear()
        {
            _available.Clear();
        }
    }
}