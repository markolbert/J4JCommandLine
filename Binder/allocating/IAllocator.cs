using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines the interface for an object used to parse a command line
    public interface IAllocator
    {
        bool AllocateCommandLine(string[] args, Options options, out List<string>? unkeyed);
        bool AllocateCommandLine(string cmdLine, Options options, out List<string>? unkeyed);
    }
}