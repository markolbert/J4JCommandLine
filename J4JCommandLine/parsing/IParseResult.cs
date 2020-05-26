using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the interface for an individual parsing result
    // (e.g., "-x param1 param2")
    public interface IParseResult
    {
        // the key defining which option is referenced by the result
        string Key { get; }
        
        // the number of parameters in the result (e.g., 2 for "-x param1 param2")
        int NumParameters { get; }

        List<string> Parameters { get; }
    }
}