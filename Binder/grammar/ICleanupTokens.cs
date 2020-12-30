using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface ICleanupTokens
    {
        void Process( List<Token> tokens );
    }
}