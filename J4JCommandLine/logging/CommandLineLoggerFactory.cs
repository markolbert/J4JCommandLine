using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class CommandLineLoggerFactory
{
    public static CommandLineLoggerFactory Default { get; } = new();

    private readonly Dictionary<Type, ICommandLineLogger> _loggers = [];

    private CommandLineLoggerFactory()
    {
    }

    public bool EnableLogging { get; set; }

    public ILogger? Create<T>()
        where T : class =>
        Create( typeof( T ) );

    public ILogger? Create( Type type )
    {
        if( !EnableLogging )
            return null;

        if( _loggers.TryGetValue( type, out var logger ) )
            return logger;

        var loggerType = typeof( CommandLineLogger<> ).MakeGenericType( type );
        logger = (ICommandLineLogger) Activator.CreateInstance( loggerType, type )!;

        _loggers.Add( type, logger );

        return logger;
    }

    public void Dump( ILogger? logger, LogLevel minLevel = LogLevel.Trace )
    {
        if( logger == null )
            return;

        foreach( var entry in _loggers.SelectMany( kvp => kvp.Value.Entries )
                                      .Where( e => e.Level >= minLevel ) )
        {
            logger.Log( entry.Level, entry.Exception, "{loggedType}:: {text}", entry.LoggedType.Name, entry.Text );
        }
    }
}
