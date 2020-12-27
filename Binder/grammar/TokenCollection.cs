using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class TokenCollection
    {
        public static TokenCollection GetDefault(
            CommandLineStyle style,
            CommandLineLogger logger,
            StringComparison? textComparison = null )
        {
            textComparison ??= style == CommandLineStyle.Linux
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            var retVal = new TokenCollection( textComparison.Value, logger );

            if( style == CommandLineStyle.UserDefined )
            {
                logger.LogError( "Requested a user-defined default TokenCollection, which contains no tokens" );
                return retVal;
            }

            retVal.Add( TokenType.Separator, " " );
            retVal.Add( TokenType.Separator, "\t" );
            retVal.Add( TokenType.ValuePrefix, "=" );

            switch( style )
            {
                case CommandLineStyle.Linux:
                    retVal.Add( TokenType.Quoter, "\"" );
                    retVal.Add( TokenType.Quoter, "'" );
                    retVal.Add( TokenType.KeyPrefix, "-" );
                    retVal.Add( TokenType.KeyPrefix, "--" );

                    break;

                case CommandLineStyle.Windows:
                    retVal.Add( TokenType.Quoter, "\"" );
                    retVal.Add(TokenType.Quoter, "'");
                    retVal.Add( TokenType.KeyPrefix, "/" );

                    break;

                case CommandLineStyle.Universal:
                    retVal.Add( TokenType.Quoter, "\"" );
                    retVal.Add( TokenType.Quoter, "'" );
                    retVal.Add( TokenType.KeyPrefix, "-" );
                    retVal.Add( TokenType.KeyPrefix, "--" );
                    retVal.Add( TokenType.KeyPrefix, "/" );

                    break;
            }

            return retVal;
        }

        private readonly List<Token> _tokens = new List<Token>();
        private readonly CommandLineLogger _logger;

        public TokenCollection( StringComparison textComp, CommandLineLogger logger )
        {
            TextComparison = textComp;
            _logger = logger;
        }

        public StringComparison TextComparison { get; }

        public bool Add( TokenType type, string text )
        {
            if( type == TokenType.EndOfInput || type == TokenType.Text )
            {
                _logger.LogError( $"Cannot include {type} tokens" );
                return false;
            }

            if( _tokens.Any( t => t.Text.Equals( text, TextComparison ) ) )
            {
                _logger.LogError( $"Duplicate token text '{text}' ({type})" );
                return false;
            }

            _tokens.Add( new Token( type, text ) );

            return true;
        }

        public bool Remove( string text )
        {
            var idx = _tokens.FindIndex( t => t.Text.Equals( text, TextComparison ) );

            if( idx < 0 )
            {
                _logger.LogError( $"Couldn't find '{text}' among tokens to delete" );
                return false;
            }

            _tokens.RemoveAt( idx );

            return true;
        }

        public void Clear() => _tokens.Clear();
        public int Count => _tokens.Count;

        public List<Token> Tokenize( string cmdLine )
        {
            var rawTokens = new List<Token>();
            TokenMatch? firstMatch = null;

            while ( cmdLine.Length > 0 )
            {
                firstMatch = null;

                foreach( var token in _tokens )
                {
                    var tokenStart = cmdLine.IndexOf( token.Text, TextComparison );

                    // If there was no match, go to next token
                    if( tokenStart < 0 )
                        continue;

                    // We have a match. If this is our first match, store it and
                    // move on to the next token.
                    if ( firstMatch == null )
                    {
                        firstMatch = new TokenMatch( token.Copy(), tokenStart );
                        continue;
                    }

                    // If the current match starts after the existing first match,
                    // move on to the next token.
                    if( tokenStart > firstMatch.StartChar )
                        continue;

                    // if the current match starts before the existing first match,
                    // make it the first match and move on to the next token
                    if( tokenStart < firstMatch.StartChar)
                    {
                        firstMatch = new TokenMatch( token.Copy(), tokenStart );
                        continue;
                    }

                    // if the current match and the existing first match both start
                    // at the same location, make the longest match the first match.
                    if( firstMatch.Token.Length < token.Length )
                    {
                        firstMatch = new TokenMatch( token.Copy(), tokenStart );
                    }
                }

                // if no first match was defined, there are no tokens remaining in the
                // command line, so embed it all in a single, final token
                firstMatch ??= new TokenMatch( new Token( TokenType.Text, cmdLine ), 0 );

                // if the first match doesn't start at index 0 there's some text ahead
                // of the first recognized token, so output it as such
                if( firstMatch.StartChar > 0 )
                    rawTokens.Add( new Token( TokenType.Text, cmdLine[ ..firstMatch.StartChar ] ) );

                rawTokens.Add( firstMatch.Token );

                cmdLine = cmdLine.Substring( firstMatch.StartChar + firstMatch.Token.Length );
            }

            return CleanupTokens( rawTokens );
        }

        private List<Token> CleanupTokens( List<Token> tokens )
        {
            var retVal = new List<Token>();

            TokenType? prevType = null;
            string? quoter = null;
            var sb = new StringBuilder();

            foreach( var token in tokens )
            {
                // ignore 2nd, 3rd, etc., consecutive separator tokens
                if (prevType == TokenType.Separator
                    && token.Type == TokenType.Separator)
                    continue;

                prevType = token.Type;

                // how we handle quoters depends on whether we've
                // already encountered one
                if( token.Type == TokenType.Quoter )
                {
                    // if this is our first quoter, store the quoter text
                    // so we'll know when the encounter the end-of-quoted text
                    if( quoter == null )
                        quoter = token.Text;
                    else
                    {
                        // if this is our second/closing quoter, check to see if it's
                        // the same text as the first/opening quoter (multiple types
                        // of quoters are allowed so we have to match them up)
                        if( token.Text.Equals( quoter, TextComparison ) )
                        {
                            // we have a match, meaning we're at the end of the quoted
                            // tokens, so create a single Text token based on the accumulated
                            // text of all the intervening tokens
                            retVal.Add( new Token( TokenType.Text, sb.ToString() ) );

                            // reset the active quoter in preparation for encountering
                            // another set of quoted tokens, and reset the StringBuilder
                            // used to accumulate the quoted text.
                            quoter = null;
                            sb.Clear();
                        }
                        // it's not the same type of quoter so store it as plain text
                        else sb.Append( token.Text );
                    }
                }
                else
                {
                    // if we're in the midst of quoted tokens accumulate the
                    // current token's text preparatory to outputting it when we
                    // encounter the closing quoter.
                    if( quoter != null )
                        sb.Append( token.Text );
                    // otherwise just output the token
                    else retVal.Add( token );
                }
            }

            // if the StringBuilder has content we must have an unmatched pair
            // of quoters, so output what's left as a text token
            if( sb.Length > 0 )
                retVal.Add( new Token(TokenType.Text, sb.ToString())  );

            // add a closing token
            retVal.Add( new Token( TokenType.EndOfInput, string.Empty ) );

            return retVal;
        }
    }
}
