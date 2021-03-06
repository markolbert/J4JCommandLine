﻿using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Tokenizer : ITokenizer
    {
        private readonly ICleanupTokens[] _cleanupProcessors;
        private readonly AvailableTokens _collection;
        private readonly IJ4JLogger? _logger;

        public Tokenizer(
            AvailableTokens collection,
            Func<IJ4JLogger>? loggerFactory,
            params ICleanupTokens[] cleanupProcessors
        )
        {
            _collection = collection;
            _logger = loggerFactory?.Invoke();

            if( cleanupProcessors.Length > 0 )
                _cleanupProcessors = cleanupProcessors;
            else
                _cleanupProcessors = new ICleanupTokens[]
                {
                    new ConsolidateQuotedText( collection.TextComparison, loggerFactory?.Invoke() ),
                    new MergeSequentialSeparators()
                };
        }

        public Tokenizer(
            CommandLineStyle style = CommandLineStyle.Windows,
            StringComparison? textComparison = null,
            Func<IJ4JLogger>? loggerFactory = null,
            params ICleanupTokens[] cleanupProcessors
        )
        {
            _logger = loggerFactory?.Invoke();

            textComparison ??= StringComparison.OrdinalIgnoreCase;
            _collection = AvailableTokens.GetDefault( style, loggerFactory, textComparison );

            if( cleanupProcessors.Length > 0 )
                _cleanupProcessors = cleanupProcessors;
            else
                _cleanupProcessors = new ICleanupTokens[]
                {
                    new ConsolidateQuotedText( textComparison.Value, loggerFactory?.Invoke() ),
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

                foreach( var (text, tokenType) in _collection.Available )
                {
                    var tokenStart = cmdLine.IndexOf( text, _collection.TextComparison );

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

                cmdLine = cmdLine[(firstMatch.startChar + firstMatch.token.Length)..];
            }

            foreach( var cleanupProc in _cleanupProcessors ) cleanupProc.Process( retVal );

            return retVal;
        }
    }
}