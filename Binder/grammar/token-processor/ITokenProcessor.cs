using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface ITokenProcessor
    {
        List<Token> ProcessTokens( List<Token> tokens );
    }
}