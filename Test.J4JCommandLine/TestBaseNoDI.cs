using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace J4JSoftware.Binder.Tests;

public class TestBaseNoDi
{
    protected TestBaseNoDi()
    {
        Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .CreateLogger();

        Logger.ForContext( GetType() );
    }

    protected ILogger Logger { get; }
    protected ILoggerFactory LoggerFactory { get; }

    protected IOption Bind<TTarget, TProp>( OptionCollection options,
        Expression<Func<TTarget, TProp>> propSelector,
        TestConfig testConfig )
        where TTarget : class, new()
    {
        var option = options.Bind( propSelector );
        option.Should().NotBeNull();

        var optConfig = testConfig.OptionConfigurations
            .FirstOrDefault( x =>
                option!.ContextPath!.Equals( x.ContextPath,
                    StringComparison.OrdinalIgnoreCase ) );

        optConfig.Should().NotBeNull();

        option!.AddCommandLineKey( optConfig!.CommandLineKey )
            .SetStyle( optConfig.Style );

        if ( optConfig.Required ) option.IsRequired();
        else option.IsOptional();

        optConfig.Option = option;

        return option;
    }

    //protected void CreateOptionsFromContextKeys(IOptionCollection options, IEnumerable<OptionConfig> optConfigs)
    //{
    //    foreach (var optConfig in optConfigs)
    //    {
    //        CreateOptionFromContextKey(options, optConfig);
    //    }
    //}

    //private void CreateOptionFromContextKey(IOptionCollection options, OptionConfig optConfig)
    //{
    //    var option = options.Add(optConfig.GetPropertyType(), optConfig.ContextPath);
    //    option.Should().NotBeNull();

    //    option!.AddCommandLineKey(optConfig.CommandLineKey)
    //        .SetStyle(optConfig.Style);

    //    if (optConfig.Required) option.IsRequired();
    //    else option.IsOptional();

    //    optConfig.Option = option;
    //}
}