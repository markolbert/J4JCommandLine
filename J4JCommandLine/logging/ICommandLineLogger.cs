using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

internal interface ICommandLineLogger : ILogger
{
    List<LogEntry> Entries { get; }
}
