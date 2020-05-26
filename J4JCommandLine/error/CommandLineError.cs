using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // class for capturing errors, keyed by the command line key associated with an
    // error (i.e., the 'x' in '-x').
    //
    // Keys in the error collection follow the case-sensitivity rule set for the overall 
    // parsing effort (defined in IParsingConfiguration)
    public class CommandLineError
    {
        public ErrorSource Source { get; internal set; }
        public List<string> Errors { get; } = new List<string>();
    }
}