using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface ITokenizer
    {
        List<Token> Tokenize( string cmdLine );
    }
}