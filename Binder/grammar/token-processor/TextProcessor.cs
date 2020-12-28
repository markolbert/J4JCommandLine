using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class TextProcessor : TokenProcessor
    {
        public TextProcessor(CommandLineLogger logger)
            : base(logger)
        {
        }

        public override List<Token> ProcessTokens(List<Token> tokens)
        {
            var retVal = new List<Token>();

            Token? prevToken = null;

            foreach (var token in tokens)
            {
                // if this is our first token, just add it
                if (prevToken == null)
                    retVal.Add(token);
                else
                {
                    // if the current token is text, and the previous token was text,
                    // merge them and replace the previous token with the merged one
                    if (prevToken.Type == TokenType.Text && token.Type == TokenType.Text)
                    {
                        var merged = new Token(TokenType.Text, $"{prevToken.Text}{token.Text}");

                        retVal.RemoveAt(retVal.Count - 1);
                        retVal.Add(merged);
                    }
                    // otherwise, just add the token
                    else retVal.Add(token);
                }

                prevToken = token;
            }

            return retVal;
        }
    }
}