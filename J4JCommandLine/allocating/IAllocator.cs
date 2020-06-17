using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the interface for an object used to parse a command line
    public interface IAllocator
    {
        bool IsInitialized { get; }

        bool Initialize(
            StringComparison keyComp,
            CommandLineLogger logger,
            MasterTextCollection masterText );
        
        IAllocations AllocateCommandLine( string[] args );
        IAllocations AllocateCommandLine( string cmdLine );
    }
}