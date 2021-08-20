using System;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IOptionsGenerator : ICustomized
    {
        bool IsInitialized { get; }

        void Initialize( StringComparison textComparision, IOptionCollection options );

        bool Create( TokenPair tokenPair );
        bool EndParsing( TokenPair tokenPair );
        bool TerminateWithPrejuidice( TokenPair tokenPair );
        bool Commit( TokenPair tokenPair );
        bool ConsumeToken( TokenPair tokenPair );
        bool ProcessText( TokenPair tokenPair );
    }
}