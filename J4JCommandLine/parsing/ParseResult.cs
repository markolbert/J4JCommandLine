using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class ParseResult : IParseResult
    {
        public string Key { get; set; } = string.Empty;
        public int NumParameters => Parameters?.Count ?? 0;
        public List<string> Parameters { get; } = new List<string>();
    }
}