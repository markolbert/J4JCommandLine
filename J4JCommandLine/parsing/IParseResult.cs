using System.Collections.Generic;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public interface IParseResult
    {
        IOption Option { get; }
        List<string> Arguments { get; }
    }
}