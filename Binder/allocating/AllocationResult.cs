using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class AllocationResult
    {
        public List<string> UnkeyedParameters { get; } = new List<string>();
        public List<string> UnknownKeys { get; } = new List<string>();
    }
}