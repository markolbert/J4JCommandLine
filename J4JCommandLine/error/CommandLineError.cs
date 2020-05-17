using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class CommandLineError
    {
        public ErrorSource Source { get; internal set; } = new ErrorSource( StringComparison.OrdinalIgnoreCase );
        public List<string> Errors { get; } = new List<string>();
    }
}