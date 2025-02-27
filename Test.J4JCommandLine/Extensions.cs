using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;

namespace J4JSoftware.Binder.Tests;

public static class Extensions
{
    public static IOption Bind<TTarget, TProp>(
        this J4JCommandLineBuilder optionBuilder,
        Expression<Func<TTarget, TProp>> propSelector,
        TestConfig config
    )
        where TTarget : class, new()
    {
        var option = optionBuilder.Bind( propSelector );
        option.Should().NotBeNull();

        var optConfig = config.OptionConfigurations
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
