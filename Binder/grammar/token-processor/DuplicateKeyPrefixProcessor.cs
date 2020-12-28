using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class DuplicateTokenProcessor : TokenProcessor
    {
        public DuplicateTokenProcessor(CommandLineLogger logger)
            : base(logger)
        {
        }

        public override List<Token> ProcessTokens(List<Token> tokens)
        {
            var retVal = new List<Token>();

            for( var idx = 0; idx < tokens.Count; idx++ )
            {
                if( idx == 0 || tokens[idx-1].Type != tokens[idx].Type )
                {
                    retVal.Add( tokens[ idx ] );
                    continue;
                }

                // discard duplicates of everything except Text tokens,
                // which should be merged
                if( tokens[ idx ].Type != TokenType.Text ) 
                    continue;

                var merged = new Token( TokenType.Text, $"{retVal.Last().Text}{tokens[ idx ].Text}" );
                retVal[ ^1 ] = merged;
            }

            return retVal;
        }
    }
}