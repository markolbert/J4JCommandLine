using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace J4JSoftware.CommandLine
{
    public class KeyPrefixProcessor : TokenProcessor
    {
        public KeyPrefixProcessor(CommandLineLogger logger)
            : base(logger)
        {
        }

        public override List<Token> ProcessTokens(List<Token> tokens)
        {
            var retVal = new List<Token>();

            for( var idx = 0; idx < tokens.Count; idx++ )
            {
                if( tokens[ idx ].Type != TokenType.KeyPrefix )
                {
                    retVal.Add( tokens[ idx ] );
                    continue;
                }

                // from here on all tokens are KeyPrefix tokens

                // KeyPrefix tokens must be followed by a Text token
                if( idx == tokens.Count - 1
                    || idx == tokens.Count - 2 && tokens[ idx + 1 ].Type != TokenType.Text )
                    break;

                if( tokens[ idx + 1 ].Type == TokenType.Text )
                    retVal.Add( tokens[ idx ] );
            }

            return retVal;
        }
    }
}