using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the interface for an object used to parse a command line
    public interface ICommandLineParser
    {
        IElementKey Prefixer { get; }
        bool IsInitialized { get; }

        bool Initialize(
            StringComparison keyComp,
            CommandLineErrors errors,
            IEnumerable<string> prefixes,
            IEnumerable<string>? enclosers = null,
            IEnumerable<char>? quoteChars = null );
        
        ParseResults Parse( string[] args );
        ParseResults Parse( string cmdLine );
    }
}