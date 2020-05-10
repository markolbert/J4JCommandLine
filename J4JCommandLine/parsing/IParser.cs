using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IParser
    {
        List<IParseResult> Parse( string[] arguments );
    }
}