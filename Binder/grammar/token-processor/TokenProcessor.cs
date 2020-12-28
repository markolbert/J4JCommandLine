using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public abstract class TokenProcessor : ITokenProcessor
    {
        protected TokenProcessor( CommandLineLogger logger )
        {
            Logger = logger;
        }

        protected CommandLineLogger Logger { get; }

        public abstract List<Token> ProcessTokens( List<Token> tokens );
    }
}