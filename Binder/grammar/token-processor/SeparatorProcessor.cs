using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class SeparatorProcessor : TokenProcessor
    {
        public SeparatorProcessor( CommandLineLogger logger )
            : base( logger )
        {
        }

        public override List<Token> ProcessTokens( List<Token> tokens )
        {
            var retVal = new List<Token>();

            TokenType? prevType = null;

            foreach (var token in tokens)
            {
                if (!prevType.HasValue)
                {
                    // if we haven't encountered any tokens yet, and we're
                    // seeing separators, skip them (i.e., trim leading
                    // separators)
                    if (token.Type != TokenType.Separator)
                        prevType = token.Type;

                    continue;
                }

                if (prevType != TokenType.Separator || token.Type != TokenType.Separator)
                    retVal.Add(token);
            }

            return retVal;
        }
    }
}