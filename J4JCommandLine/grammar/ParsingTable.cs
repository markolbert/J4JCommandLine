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
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public delegate bool ParsingAction(TokenPair tokenPair);

    public class ParsingTable : IParsingTable
    {
        private readonly Dictionary<TokenType, Dictionary<TokenType, ParsingAction?>> _table =
            new();

        private readonly ParsingAction _endParsing;
        private readonly IJ4JLogger? _logger;

        internal ParsingTable( 
            IOptionsGenerator generator,
            IJ4JLogger? logger = null 
            )
        {
            _endParsing = generator.EndParsing;
            
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            foreach( var row in Enum.GetValues<TokenType>() )
            {
                // Quoter never appears in the list of processed tokens because
                // they are eliminated by the token cleanup routine
                if( row == TokenType.Quoter )
                    continue;

                _table.Add( row, new Dictionary<TokenType, ParsingAction?>() );

                foreach( var col in Enum.GetValues<TokenType>() )
                {
                    // StartOfInput is only a row entry, not a column entry,
                    // because it can never appear as a "next" token
                    // Quoter never appears in the list of processed tokens because
                    // they are eliminated by the token cleanup routine
                    // EndOfInput we handle specially when the handler is requested
                    if( col switch
                    {
                        TokenType.StartOfInput => true,
                        TokenType.Quoter => true,
                        TokenType.EndOfInput => true,
                        _ => false
                    } )
                        continue;

                    _table[ row ].Add( col, null );
                }
            }

            _table[ TokenType.StartOfInput][TokenType.KeyPrefix ] = generator.Create;
            _table[ TokenType.StartOfInput][TokenType.ValuePrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.StartOfInput][TokenType.Separator ] = generator.ConsumeToken;
            _table[ TokenType.StartOfInput][TokenType.Text ] = generator.ProcessText;

            _table[ TokenType.KeyPrefix][TokenType.KeyPrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.KeyPrefix][TokenType.ValuePrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.KeyPrefix][TokenType.Separator ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.KeyPrefix][TokenType.Text ] = generator.ProcessText;

            _table[ TokenType.ValuePrefix][TokenType.KeyPrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.ValuePrefix][TokenType.ValuePrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.ValuePrefix][TokenType.Separator ] = generator.ConsumeToken;
            _table[ TokenType.ValuePrefix][TokenType.Text ] = generator.ProcessText;

            _table[ TokenType.Separator][TokenType.KeyPrefix ] = generator.Commit;
            _table[ TokenType.Separator][TokenType.ValuePrefix ] = generator.ConsumeToken;
            _table[ TokenType.Separator][TokenType.Separator ] = generator.ConsumeToken;
            _table[ TokenType.Separator][TokenType.Text ] = generator.ProcessText;

            _table[ TokenType.Text][TokenType.KeyPrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.Text][TokenType.ValuePrefix ] = generator.TerminateWithPrejuidice;
            _table[ TokenType.Text][TokenType.Separator ] = generator.ConsumeToken;
            _table[ TokenType.Text][TokenType.Text ] = generator.ProcessText;
        }

        public bool IsValid => !_table
            .Any( row =>
                row.Value.Any( col =>
                    col.Value == null ) );

        public ParsingAction? this[ TokenTypePair typePair ]
        {
            get => typePair.Current == TokenType.EndOfInput
                    ? _endParsing
                    : _table[ typePair.Previous ][ typePair.Current ];
            set => _table[ typePair.Previous ][ typePair.Current ] = value;
        }
    }
}