using System;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Serilog;

namespace J4JSoftware.Binder.Tests;

public class TestBase
{
    private readonly ContainerBuilder _builder = new();

    protected TestBase()
    {
        Container = Configure();

        Logger = Container.Resolve<ILogger>();
        Logger.ForContext(GetType());
    }

    protected IContainer Container { get; }
    protected ILogger Logger { get; }

    private IContainer Configure()
    {
        ConfigureContainer( _builder );

        return _builder.Build();
    }

    protected virtual void ConfigureContainer( ContainerBuilder builder )
    {
        builder.Register( _ => new LoggerConfiguration()
                .WriteTo.Debug()
                .CreateLogger())
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    //private void RegisterBuiltInTextToValue( ContainerBuilder builder )
    //{
    //    foreach ( var convMethod in typeof( Convert )
    //                 .GetMethods( BindingFlags.Static | BindingFlags.Public )
    //                 .Where( m =>
    //                 {
    //                     var parameters = m.GetParameters();

    //                     return parameters.Length == 1
    //                            && !typeof( string ).IsAssignableFrom( parameters[ 0 ]
    //                                .ParameterType );
    //                 } ) )
    //    {
    //        builder.Register( c =>
    //        {
    //            var logger = c.IsRegistered<ILogger>() ? c.Resolve<ILogger>() : null;

    //            var builtInType =
    //                typeof( BuiltInTextToValue<> ).MakeGenericType( convMethod.ReturnType );

    //            return (ITextToValue) Activator.CreateInstance( builtInType,
    //                new object?[] { convMethod, logger } )!;
    //        } );
    //    }
    //}

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
}