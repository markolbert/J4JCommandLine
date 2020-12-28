using System;
using System.Collections.Generic;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class QuotationProcessor : TokenProcessor
    {
        private readonly StringComparison _textComparison;

        public QuotationProcessor(
            StringComparison textComparison,
            CommandLineLogger logger )
            : base( logger )
        {
            _textComparison = textComparison;
        }

        public override List<Token> ProcessTokens(List<Token> tokens)
        {
            var retVal = new List<Token>();
            string? quoter = null;
            var sb = new StringBuilder();

            foreach (var token in tokens)
            {
                // how we handle quoters depends on whether we've
                // already encountered one
                if (token.Type == TokenType.Quoter)
                {
                    // if this is our first quoter, store the quoter text
                    // so we'll know when the encounter the end-of-quoted text
                    if (quoter == null)
                        quoter = token.Text;
                    else
                    {
                        // if this is our second/closing quoter, check to see if it's
                        // the same text as the first/opening quoter (multiple types
                        // of quoters are allowed so we have to match them up)
                        if (token.Text.Equals(quoter, _textComparison))
                        {
                            // we have a match, meaning we're at the end of the quoted
                            // tokens, so create a single Text token based on the accumulated
                            // text of all the intervening tokens
                            retVal.Add(new Token(TokenType.Text, sb.ToString()));

                            // reset the active quoter in preparation for encountering
                            // another set of quoted tokens, and reset the StringBuilder
                            // used to accumulate the quoted text.
                            quoter = null;
                            sb.Clear();
                        }
                        // it's not the same type of quoter so store it as plain text
                        else sb.Append(token.Text);
                    }
                }
                else
                {
                    // if we're in the midst of quoted tokens accumulate the
                    // current token's text preparatory to outputting it when we
                    // encounter the closing quoter.
                    if (quoter != null)
                        sb.Append(token.Text);
                    // otherwise just output the token
                    else retVal.Add(token);
                }
            }

            // if the StringBuilder has content we must have an unmatched pair
            // of quoters, so output what's left as a text token
            if (sb.Length > 0)
                retVal.Add(new Token(TokenType.Text, sb.ToString()));

            return retVal;
        }
    }
}