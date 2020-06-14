using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class ParseResult : IParseResult
    {
        public ParseResult( IParseResults container )
        {
            Container = container;
        }

        public IParseResults Container { get; }

        public bool IsLastResult => Container.Where( ( pr, i ) => pr == this && i == Container.Count - 1 )
            .FirstOrDefault() != null;

        public string Key { get; set; } = string.Empty;
        public int NumParameters => Parameters?.Count ?? 0;
        public List<string> Parameters { get; private set; } = new List<string>();

        public void MoveExcessParameters( int toKeep )
        {
            Container.Unkeyed.Parameters.AddRange( Parameters.Skip( toKeep ) );

            Parameters = Parameters.Take( toKeep ).ToList();
        }
    }
}