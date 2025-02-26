using System;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

internal record LogEntry(LogLevel Level, Type LoggedType, string? Text, Exception? Exception);
