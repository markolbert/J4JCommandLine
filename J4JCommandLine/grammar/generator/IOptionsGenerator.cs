using System;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IOptionsGenerator
    {
        bool Create( TokenPair tokenPair );
        bool EndParsing( TokenPair tokenPair );
        bool TerminateWithPrejuidice( TokenPair tokenPair );
        bool Commit( TokenPair tokenPair );
        bool ConsumeToken( TokenPair tokenPair );
        bool ProcessText( TokenPair tokenPair );
    }
}