using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IParseResult
    {
        string Key { get; }
        int NumParameters { get; }
        List<string> Parameters { get; }
        string ParametersToText();
    }
}