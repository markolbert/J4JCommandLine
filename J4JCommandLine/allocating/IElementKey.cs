using System;

namespace J4JSoftware.CommandLine.Deprecated
{
    public interface IElementKey
    {
        bool IsInitialized { get; }

        void Initialize( 
            StringComparison textComp, 
            CommandLineLogger logger,
            MasterTextCollection masterText );

        int GetMaxPrefixLength( string text );
    }
}
