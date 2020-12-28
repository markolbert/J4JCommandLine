using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface ITokenizer
    {
        List<Token> Tokenize( string cmdLine );
    }
}