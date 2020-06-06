using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IElementTerminator
    {
        UniqueText ValueEnclosers { get; }
        ReadOnlyCollection<char> QuoteCharacters { get; }
        bool IsInitialized { get; }

        void Initialize(
            StringComparison textComp,
            CommandLineErrors errors,
            IEnumerable<string>? enclosers = null,
            IEnumerable<char>? quoteChars = null );

        int GetMaxTerminatorLength( string text, bool isKey );
    }
}