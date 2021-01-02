using System;

namespace J4JSoftware.CommandLine.Deprecated
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