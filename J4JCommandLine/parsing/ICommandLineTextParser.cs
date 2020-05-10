using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface ICommandLineTextParser
    {
        Dictionary<string, List<string>> Parse( string[] args );
    }
}