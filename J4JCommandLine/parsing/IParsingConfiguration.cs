using System;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IParsingConfiguration
    {
        ReadOnlyCollection<string> Prefixes { get; }
        ReadOnlyCollection<string> ValueEnclosers { get; }
        ReadOnlyCollection<string> TextDelimiters { get; }
        StringComparison TextComparison { get; }
    }
}