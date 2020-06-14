using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the interface for an individual parsing result
    // (e.g., "-x param1 param2")
    public interface IParseResult
    {
        // the collection of IParseResults to which this instance belongs
        IParseResults Container { get; }

        // flag indicating whether or not this instance is the last one in the container
        bool IsLastResult { get; }
        
        // the key defining which option is referenced by the result. If null
        // the IParseResult refers to the unkeyed/non-optioned parameters
        string? Key { get; }
        
        // the number of parameters in the result (e.g., 2 for "-x param1 param2")
        int NumParameters { get; }

        List<string> Parameters { get; }

        // shift excess parameters to the containing ParseResults Unkeyed property
        void MoveExcessParameters( int toKeep );
    }
}