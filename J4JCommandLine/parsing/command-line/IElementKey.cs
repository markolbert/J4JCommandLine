using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IElementKey
    {
        UniqueText Prefixes { get; }
        bool IsInitialized { get; }

        void Initialize( StringComparison textComp, params string[] prefixers );
        int GetMaxPrefixLength( string text );
    }
}
