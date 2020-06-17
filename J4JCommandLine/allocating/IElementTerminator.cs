using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IElementTerminator
    {
        bool IsInitialized { get; }

        void Initialize(
            StringComparison textComp,
            CommandLineLogger logger,
            MasterTextCollection masterText );

        int GetMaxTerminatorLength( string text, bool isKey );
    }
}