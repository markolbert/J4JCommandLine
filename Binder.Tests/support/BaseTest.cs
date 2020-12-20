using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class BaseTest
    {
        private CommandLineLogger? _cmdLineErrors;
        private IConfigurationBuilder? _configBuilder;
        
        protected TestConfig? TestConfig { get; private set; }
        protected IAllocator? Allocator { get; private set; }
        protected OptionCollection? Options { get; private set; }

        protected void Initialize( TestConfig testConfig )
        {
            TestConfig = testConfig;

            _configBuilder = new ConfigurationBuilder().AddJ4JCommandLineWindows(
                TestConfig!.CommandLine,
                out var options,
                out var allocator,
                out var errors );

            errors.HasMessages.Should().BeFalse();

            Options = options;
            Allocator = allocator;
        }

        protected void Bind<TTarget, TProp>( Expression<Func<TTarget, TProp>> propSelector )
            where TTarget : class, new()
        {
            Options!.Bind( propSelector, out var option  )
                .Should()
                .BeTrue();

            var optConfig = TestConfig!.OptionConfigurations
                .FirstOrDefault( x =>
                    option!.ContextPath!.Equals( x.ContextPath, StringComparison.OrdinalIgnoreCase ) );

            optConfig.Should().NotBeNull();
            
            option!.AddCommandLineKey(optConfig!.CommandLineKey)
                .SetStyle(optConfig.Style);

            if (optConfig.Required) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;
        }

        protected void ValidateAllocations()
        {
            var result = Allocator!.AllocateCommandLine(TestConfig!.CommandLine!, Options!);

            result.UnknownKeys.Count.Should().Be( TestConfig.UnknownKeys );
            result.UnkeyedParameters.Count.Should().Be( TestConfig.UnkeyedParameters );

            foreach( var optConfig in TestConfig.OptionConfigurations )
            {
                optConfig.Option!.ValuesSatisfied.Should().Be( optConfig.ValuesSatisfied );
            }
        }

        protected void ValidateConfiguration<TParsed>()
            where TParsed : class, new()
        {
            var config = _configBuilder!.Build();
            
            TParsed? parsed = null;

            if( TestConfig!.OptionConfigurations.Any( x => x.ParsingWillFail ) )
            {
                var exception = Assert.Throws<InvalidOperationException>( () => config.Get<TParsed>() );
                return;
            }
            
            parsed = config.Get<TParsed>();

            if( TestConfig.OptionConfigurations.TrueForAll( x => !x.ValuesSatisfied )
                && TestConfig.OptionConfigurations.All( x => x.Style != OptionStyle.Switch ) )
            {
                parsed.Should().BeNull();
                return;
            }

            foreach( var optConfig in TestConfig.OptionConfigurations)
            {
                GetPropertyValue<TParsed>( parsed, optConfig.ContextPath, out var result, out var resultType )
                    .Should()
                    .BeTrue();

                if( optConfig.Style == OptionStyle.Collection )
                {
                    if( optConfig.CorrectTextArray.Count == 0 )
                        result.Should().BeNull();
                    else result.Should().BeEquivalentTo(optConfig.CorrectTextArray);
                }
                else
                {
                    if( optConfig.CorrectText == null )
                        result.Should().BeNull();
                    else
                    {
                        var correctValue = resultType!.IsEnum
                            ? Enum.Parse( resultType, optConfig.CorrectText )
                            : Convert.ChangeType( optConfig.CorrectText, resultType! );

                        result.Should().Be( correctValue );
                    }
                }
            }
        }

        private bool GetPropertyValue<TParsed>( 
            TParsed parsed, 
            string contextKey, 
            out object? result,
            out Type? resultType )
            where TParsed : class, new()
        {
            result = null;
            resultType = null;

            Type curType = typeof(TParsed);
            object? curValue = parsed;
            
            var keys = contextKey.Split( ":", StringSplitOptions.RemoveEmptyEntries );

            for(var idx = 0; idx < keys.Length; idx++ )
            {
                var curPropInfo = curType.GetProperty( keys[idx] );
                
                if( curPropInfo == null )
                    return false;

                curType = curPropInfo.PropertyType;
                curValue = curPropInfo.GetValue( curValue );

                if( curValue == null && idx != keys.Length - 1 )
                    return false;
            }

            result = curValue;
            resultType = curType;

            return true;
        }
    }
}