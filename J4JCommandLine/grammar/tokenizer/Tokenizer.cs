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
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Tokenizer : ITokenizer
    {
        private readonly StringComparison _textComparison;
        private readonly ICleanupTokens[] _cleanupProcessors;
        private readonly IAvailableTokens _tokens;
        private readonly IJ4JLogger? _logger;

        internal Tokenizer(
            IAvailableTokens tokens,
            IJ4JLoggerFactory? loggerFactory,
            params ICleanupTokens[] cleanupProcessors
        )
        {
            _textComparison = tokens.TextComparison;
            _tokens = tokens;

            _logger = loggerFactory?.CreateLogger( GetType() );

            if( cleanupProcessors.Length > 0 )
                _cleanupProcessors = cleanupProcessors;
            else
                _cleanupProcessors = new ICleanupTokens[]
                {
                    new ConsolidateQuotedText( _textComparison, loggerFactory?.CreateLogger<ConsolidateQuotedText>() ),
                    new MergeSequentialSeparators()
                };
        }

        public List<Token> Tokenize( string cmdLine )
        {
            var retVal = new List<Token>();

            (Token? token, int startChar) firstMatch = ( null, 0 );

            while( cmdLine.Length > 0 )
            {
                firstMatch = ( null, 0 );

                foreach( var (text, tokenType) in _tokens.Available )
                {
                    var tokenStart = cmdLine.IndexOf( text, _textComparison );

                    // If there was no match, go to next token
                    if( tokenStart < 0 )
                        continue;

                    // We have a match. If this is our first match, store it and
                    // move on to the next token.
                    if( firstMatch.token == null )
                    {
                        firstMatch = ( new Token( tokenType, text ), tokenStart );
                        continue;
                    }

                    // If the current match starts after the existing first match,
                    // move on to the next token.
                    if( tokenStart > firstMatch.startChar )
                        continue;

                    // if the current match starts before the existing first match,
                    // make it the first match and move on to the next token
                    if( tokenStart < firstMatch.startChar )
                    {
                        firstMatch = ( new Token( tokenType, text ), tokenStart );
                        continue;
                    }

                    // if the current match and the existing first match both start
                    // at the same location, make the longest match the first match.
                    if( firstMatch.token.Length < text.Length )
                        firstMatch = ( new Token( tokenType, text ), tokenStart );
                }

                // if no first match was defined, there are no tokens remaining in the
                // command line, so embed it all in a single, final token
                if( firstMatch.token == null )
                    firstMatch = ( new Token( TokenType.Text, cmdLine ), 0 );

                // if the first match doesn't start at index 0 there's some text ahead
                // of the first recognized token, so output it as such
                if( firstMatch.startChar > 0 )
                    retVal.Add( new Token( TokenType.Text, cmdLine[ ..firstMatch.startChar ] ) );

                retVal.Add( firstMatch.token );

                cmdLine = cmdLine[ ( firstMatch.startChar + firstMatch.token.Length ).. ];
            }

            foreach( var cleanupProc in _cleanupProcessors )
            {
                cleanupProc.Process( retVal );
            }

            return retVal;
        }
    }
}