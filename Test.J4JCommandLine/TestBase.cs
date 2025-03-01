using System;
using System.Linq;
using Autofac;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;
using Serilog;
using Xunit;
using ILogger = Serilog.ILogger;

namespace J4JSoftware.Binder.Tests;

public class TestBase
{
    protected TestBase()
    {
        var builder = new ContainerBuilder();

        ConfigureContainerInternal( builder );
        Container = builder.Build();

        Logger = Container.Resolve<ILogger>();
        Logger.ForContext( GetType() );
    }

    protected IContainer? Container { get; }
    protected ILogger? Logger { get; }

    protected J4JCommandLineBuilder GetOptionBuilder( string osName, string cmdLineText, params ICleanupTokens[] cleanupTokens )
    {
        var os = osName.Equals( "windows", StringComparison.OrdinalIgnoreCase )
            ? CommandLineOperatingSystems.Windows
            : CommandLineOperatingSystems.Linux;

        var textComparison = os == CommandLineOperatingSystems.Windows
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        var retVal = new J4JCommandLineBuilder( textComparison, os ) { CommandLineText = cmdLineText };
        retVal.CleanupTokens.AddRange( cleanupTokens );

        return retVal;
    }

    protected (J4JCommandLineBuilder optionBuilder, IParser parser) GetOptionBuilderAndParser(
        string osName,
        string cmdLineText,
        params ICleanupTokens[] cleanupTokens
    )
    {
        var optionBuilder = GetOptionBuilder( osName, cmdLineText, cleanupTokens );

        var parser = osName.Equals( "windows", StringComparison.OrdinalIgnoreCase )
            ? Parser.GetWindowsDefault( optionBuilder )
            : Parser.GetLinuxDefault( optionBuilder );

        return ( optionBuilder, parser );
    }

    private void ConfigureContainerInternal( ContainerBuilder builder )
    {
        ConfigureContainer(builder);
    }

    protected virtual void ConfigureContainer( ContainerBuilder builder )
    {
        builder.Register( _ => new LoggerConfiguration()
                              .WriteTo.Debug()
                              .CreateLogger() )
               .AsImplementedInterfaces()
               .SingleInstance();
    }

    protected void ValidateConfiguration<TParsed>(TestConfig config, J4JCommandLineBuilder optionBuilder )
        where TParsed : class, new()
    {
        var configBuilder = new ConfigurationBuilder()
           .AddJ4JCommandLine( optionBuilder );

        var configRoot = configBuilder.Build();

        if (config.OptionConfigurations.Any(x => x.ConversionWillFail))
        {
            // ReSharper disable once UnusedVariable
            var exception = Assert.Throws<InvalidOperationException>(configRoot.Get<TParsed>);
            return;
        }

        var parsed = configRoot.Get<TParsed>();

        if (config.OptionConfigurations.TrueForAll(x => !x.ValuesSatisfied))
            return;

        parsed.Should().NotBeNull();

        foreach (var optConfig in config.OptionConfigurations)
        {
            GetPropertyValue(parsed, optConfig.ContextPath, out var result, out var resultType)
               .Should()
               .BeTrue();

            if (optConfig.Style == OptionStyle.Collection)
            {
                if (optConfig.CorrectTextArray.Count == 0)
                    result.Should().BeNull();
                else result.Should().BeEquivalentTo(optConfig.CorrectTextArray);
            }
            else
            {
                if (optConfig.CorrectText == null)
                {
                    if (optConfig.ValuesSatisfied)
                        result.Should().BeNull();
                }
                else
                {
                    var correctValue = resultType!.IsEnum
                        ? Enum.Parse(resultType, optConfig.CorrectText)
                        : Convert.ChangeType(optConfig.CorrectText, resultType);

                    result.Should().Be(correctValue);
                }
            }
        }
    }

    private bool GetPropertyValue<TParsed>(
        TParsed parsed,
        string contextKey,
        out object? result,
        out Type? resultType
    )
        where TParsed : class?, new()
    {
        result = null;
        resultType = null;

        var curType = typeof(TParsed);
        object? curValue = parsed;

        var keys = contextKey.Split(":", StringSplitOptions.RemoveEmptyEntries);

        for (var idx = 0; idx < keys.Length; idx++)
        {
            var curPropInfo = curType.GetProperty(keys[idx]);

            if (curPropInfo == null)
                return false;

            curType = curPropInfo.PropertyType;
            curValue = curPropInfo.GetValue(curValue);

            if (curValue == null && idx != keys.Length - 1)
                return false;
        }

        result = curValue;
        resultType = curType;

        return true;
    }

    protected void ValidateTokenizing(TestConfig config, IParser parser)
    {
        parser.Parse(config.CommandLine)
              .Should()
              .BeTrue();

        parser.Collection.UnknownKeys.Count.Should().Be(config.UnknownKeys);
        parser.Collection.SpuriousValues.Count.Should().Be(config.UnkeyedValues);

        foreach (var optConfig in config.OptionConfigurations)
        {
            optConfig.Option!
                     .ValuesSatisfied
                     .Should()
                     .Be(optConfig.ValuesSatisfied);
        }
    }

}
