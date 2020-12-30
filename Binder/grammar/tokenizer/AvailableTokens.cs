using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class AvailableTokens
    {
        private readonly Dictionary<TokenType, List<string>> _available = new();
        private readonly CommandLineLogger _logger;

        public AvailableTokens( StringComparison textComp, CommandLineLogger logger )
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
                _logger.LogError( $"Cannot include {type} tokens" );
                return false;
            }

            if( _available.SelectMany( kvp => kvp.Value )
                .Any( t => t.Equals( text, TextComparison ) ) )
            {
                _logger.LogError( $"Duplicate token text '{text}' ({type})" );
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
                _logger.LogError( $"Couldn't find '{text}' among tokens to delete" );
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