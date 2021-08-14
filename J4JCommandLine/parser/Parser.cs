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

using System.Runtime.InteropServices.ComTypes;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Parser : IParser
    {
        private readonly ParsingTable _parsingTable;
        private readonly IJ4JLogger? _logger;

        internal Parser(
            ParsingTable parsingTable,
            ITokenizer tokenizer,
            IJ4JLogger? logger
        )
        {
            _parsingTable = parsingTable;
            Tokenizer = tokenizer;

            _logger = logger;
        }

        public ITokenizer Tokenizer { get; }
        public IOptionCollection Options => _parsingTable.Options;

        public bool Parse( string cmdLine )
        {
            var prevToken = new Token( TokenType.StartOfInput, string.Empty );

            var allOkay = true;

            foreach( var token in Tokenizer!.Tokenize( cmdLine ) )
            {
                var parsingAction = _parsingTable[ prevToken.Type, token.Type ];

                if( parsingAction == null )
                {
                    _logger?.Error( "Undefined parsing action for (row, column) '({0}, {1})'", 
                        prevToken.Type,
                        token.Type );

                    return false;
                }

                allOkay &= parsingAction( prevToken, token, token.Text );

                prevToken = token;
            }

            // always end processing with a commit because there will generally be
            // a pending entry
            allOkay &= _parsingTable.Entries.Commit( prevToken, prevToken, string.Empty );

            return allOkay;
        }
    }
}