#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Tokenizer.cs
//
// This file is part of JumpForJoy Software's J4JCommandLine.
// 
// J4JCommandLine is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JCommandLine is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JCommandLine. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class Tokenizer : ITokenizer
{
    public static Tokenizer GetWindowsDefault(
        ILoggerFactory? loggerFactory = null,
        params ICleanupTokens[] cleanupProcessors
    ) =>
        new( new WindowsLexicalElements( loggerFactory ), loggerFactory, cleanupProcessors );

    public static Tokenizer GetLinuxDefault(
        ILoggerFactory? loggerFactory = null,
        params ICleanupTokens[] cleanupProcessors
    ) =>
        new( new LinuxLexicalElements( loggerFactory ), loggerFactory, cleanupProcessors );

    private readonly ICleanupTokens[] _cleanupProcessors;
    private readonly ILexicalElements _tokens;

    public Tokenizer(
        ILexicalElements tokens,
        ILoggerFactory? loggerFactory = null,
        params ICleanupTokens[] cleanupProcessors
    )
    {
        TextComparison = tokens.TextComparison;
        _tokens = tokens;

        if( cleanupProcessors.Length > 0 )
            _cleanupProcessors = cleanupProcessors;
        else
        {
            _cleanupProcessors =
            [
                new ConsolidateQuotedText( TextComparison, loggerFactory ),
                new MergeSequentialSeparators()
            ];
        }
    }

    public StringComparison TextComparison { get; }

    public List<Token> Tokenize( string cmdLine )
    {
        var retVal = new List<Token>();

        while( cmdLine.Length > 0 )
        {
            (Token? token, int startChar) firstMatch = ( null, 0 );

            foreach( var token in _tokens )
            {
                var tokenStart = cmdLine.IndexOf( token.Text, TextComparison );

                // If there was no match, go to next token
                if( tokenStart < 0 )
                    continue;

                // We have a match. If this is our first match, store it and
                // move on to the next token.
                if( firstMatch.token == null )
                {
                    firstMatch = ( token.Copy(), tokenStart );
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
                    firstMatch = ( token.Copy(), tokenStart );
                    continue;
                }

                // if the current match and the existing first match both start
                // at the same location, make the longest match the first match.
                if( firstMatch.token.Length < token.Length )
                    firstMatch = ( token.Copy(), tokenStart );
            }

            // if no first match was defined, there are no tokens remaining in the
            // command line, so embed it all in a single, final token
            if( firstMatch.token == null )
                firstMatch = ( new Token( LexicalType.Text, cmdLine ), 0 );

            // if the first match doesn't start at index 0 there's some text ahead
            // of the first recognized token, so output it as such
            if( firstMatch.startChar > 0 )
                retVal.Add( new Token( LexicalType.Text, cmdLine[ ..firstMatch.startChar ] ) );

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
