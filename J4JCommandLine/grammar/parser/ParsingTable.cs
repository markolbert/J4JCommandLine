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
        private readonly Dictionary<LexicalType, Dictionary<LexicalType, ParsingAction?>> _table =
            new();

        private readonly ParsingAction _endParsing;
        private readonly IJ4JLogger? _logger;

        public ParsingTable( 
            IOptionsGenerator generator,
            IJ4JLogger? logger = null 
            )
        {
            _endParsing = generator.EndParsing;
            
            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            foreach( var row in Enum.GetValues<LexicalType>() )
            {
                // Quoter never appears in the list of processed tokens because
                // they are eliminated by the token cleanup routine
                if( row == LexicalType.Quoter )
                    continue;

                _table.Add( row, new Dictionary<LexicalType, ParsingAction?>() );

                foreach( var col in Enum.GetValues<LexicalType>() )
                {
                    // StartOfInput is only a row entry, not a column entry,
                    // because it can never appear as a "next" token
                    // Quoter never appears in the list of processed tokens because
                    // they are eliminated by the token cleanup routine
                    // EndOfInput we handle specially when the handler is requested
                    if( col switch
                    {
                        LexicalType.StartOfInput => true,
                        LexicalType.Quoter => true,
                        LexicalType.EndOfInput => true,
                        _ => false
                    } )
                        continue;

                    _table[ row ].Add( col, null );
                }
            }

            _table[ LexicalType.StartOfInput][LexicalType.KeyPrefix ] = generator.Create;
            _table[ LexicalType.StartOfInput][LexicalType.ValuePrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.StartOfInput][LexicalType.Separator ] = generator.ConsumeToken;
            _table[ LexicalType.StartOfInput][LexicalType.Text ] = generator.ProcessText;

            _table[ LexicalType.KeyPrefix][LexicalType.KeyPrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.KeyPrefix][LexicalType.ValuePrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.KeyPrefix][LexicalType.Separator ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.KeyPrefix][LexicalType.Text ] = generator.ProcessText;

            _table[ LexicalType.ValuePrefix][LexicalType.KeyPrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.ValuePrefix][LexicalType.ValuePrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.ValuePrefix][LexicalType.Separator ] = generator.ConsumeToken;
            _table[ LexicalType.ValuePrefix][LexicalType.Text ] = generator.ProcessText;

            _table[ LexicalType.Separator][LexicalType.KeyPrefix ] = generator.Commit;
            _table[ LexicalType.Separator][LexicalType.ValuePrefix ] = generator.ConsumeToken;
            _table[ LexicalType.Separator][LexicalType.Separator ] = generator.ConsumeToken;
            _table[ LexicalType.Separator][LexicalType.Text ] = generator.ProcessText;

            _table[ LexicalType.Text][LexicalType.KeyPrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.Text][LexicalType.ValuePrefix ] = generator.TerminateWithPrejudice;
            _table[ LexicalType.Text][LexicalType.Separator ] = generator.ConsumeToken;
            _table[ LexicalType.Text][LexicalType.Text ] = generator.ProcessText;
        }

        public bool IsValid => !_table
            .Any( row =>
                row.Value.Any( col =>
                    col.Value == null ) );

        public ParsingAction? this[ LexicalPair typePair ]
        {
            get => typePair.Current == LexicalType.EndOfInput
                    ? _endParsing
                    : _table[ typePair.Previous ][ typePair.Current ];
            set => _table[ typePair.Previous ][ typePair.Current ] = value;
        }
    }
}