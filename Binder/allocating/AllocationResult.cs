using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class AllocationResult
    {
        public static implicit operator bool( AllocationResult result ) => result.UnknownKeys.Count <= 0;

        public List<string> UnkeyedParameters { get; } = new List<string>();
        public List<string> UnknownKeys { get; } = new List<string>();
    }
}