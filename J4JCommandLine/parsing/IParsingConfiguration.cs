using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IParsingConfiguration
    {
        List<string> Prefixes { get; }
        List<string> ValueEnclosers { get; }
        List<string> TextDelimiters { get; }
        List<string> HelpKeys { get; }
        StringComparison TextComparison { get; }
        string? Description { get; }
        string? ProgramName { get; }

    }
}