using System;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace J4JSoftware.Binder.Tests;

public class TestBase
{
    private readonly ContainerBuilder _builder = new();

    protected TestBase()
    {
        Container = Configure();

        Logger = Container.Resolve<ILogger>();
        Logger.ForContext( GetType() );
    }

    protected IContainer Container { get; }
    protected ILogger Logger { get; }
    protected ILoggerFactory LoggerFactory { get; }

    private IContainer Configure()
    {
        ConfigureContainer( _builder );

        return _builder.Build();
    }

    protected virtual void ConfigureContainer( ContainerBuilder builder )
    {
        builder.Register( _ => new LoggerConfiguration()
                              .WriteTo.Debug()
                              .CreateLogger() )
               .AsImplementedInterfaces()
               .SingleInstance();
    }

    protected IOption Bind<TTarget, TProp>(
        OptionCollection options,
        Expression<Func<TTarget, TProp>> propSelector,
        TestConfig testConfig
    )
        where TTarget : class, new()
    {
        var option = options.Bind( propSelector );
        option.Should().NotBeNull();

        var optConfig = testConfig.OptionConfigurations
                                  .FirstOrDefault( x =>
                                                       option.ContextPath!.Equals( x.ContextPath,
                                                           StringComparison.OrdinalIgnoreCase ) );

        optConfig.Should().NotBeNull();

        option.AddCommandLineKey( optConfig.CommandLineKey )
              .SetStyle( optConfig.Style );

        if( optConfig.Required ) option.IsRequired();
        else option.IsOptional();

        optConfig.Option = option;

        return option;
    }
}
