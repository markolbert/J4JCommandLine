using System.Collections.Generic;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public interface IParseResult
    {
        string Key { get; }
        List<string> Arguments { get; }
    }
}