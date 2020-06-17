using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
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
