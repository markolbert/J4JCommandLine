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

using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Parser : IParser
    {
        private readonly ParsingTable _parsingTable;
        private readonly IJ4JLogger? _logger;

        internal Parser(
            IOptionCollection options,
            ParsingTable parsingTable,
            ITokenizer tokenizer,
            IJ4JLogger? logger
        )
        {
            Options = options;
            _parsingTable = parsingTable;
            Tokenizer = tokenizer;

            _logger = logger;
        }

        public ITokenizer Tokenizer { get; }
        public IOptionCollection Options { get; }

        public bool Parse( string cmdLine )
        {
            var tokenList = Tokenizer!.Tokenize( cmdLine );

            foreach( var tokenPair in tokenList.EnumerateTokenPairs() )
            {
                var parsingAction = _parsingTable[ tokenPair.TokenTypePair ];

                if( parsingAction == null )
                {
                    _logger?.Error( "Undefined parsing action for token sequence '{0} => {1}'", 
                        tokenPair.Previous.Type,
                        tokenPair.Current.Type );

                    return false;
                }

                if( !parsingAction( tokenPair ) )
                    return false;
            }

            return true;
        }
    }
}