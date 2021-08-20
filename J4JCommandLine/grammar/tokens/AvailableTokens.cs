#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class AvailableTokens : CustomizedEntity, IAvailableTokens
    {
        private readonly Dictionary<TokenType, List<string>> _available = new();

        protected AvailableTokens( 
            IJ4JLogger? logger )
        :base(false)
        {
            Logger = logger;
        }

        protected IJ4JLogger? Logger { get; }

        public virtual void Initialize()
        {
            _available.Clear();

            // common tokens
            Add(TokenType.Separator, " ");
            Add(TokenType.Separator, "\t");
            Add(TokenType.ValuePrefix, "=");
        }

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
                Logger?.Error( "Cannot include {0} tokens", type );
                return false;
            }

            if( _available.SelectMany( kvp => kvp.Value )
                .Any( t => t.Equals( text, TextComparison ) ) )
            {
                Logger?.Error( "Duplicate token text '{0}' ({1})", text, type );
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
                Logger?.Error<string>( "Couldn't find '{0}' among tokens to delete", text );
                return false;
            }

            kvp.Value.RemoveAt( idx );

            return true;
        }
    }
}