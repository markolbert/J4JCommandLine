using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class BaseTest
    {
        private Options? _options;
        private IAllocator? _allocator;
        private TestConfig? _testConfig;

        protected void Initialize( TestConfig config )
        {
            _options = CompositionRoot.Default.GetOptions();
            _allocator = CompositionRoot.Default.GetAllocator();
            _testConfig = config;

            foreach( var optConfig in config.OptionConfigurations )
            {
                optConfig.CreateOption( _options! );
            }
        }

        protected void ValidateAllocations()
        {
            var result = _allocator!.AllocateCommandLine(_testConfig!.CommandLine!, _options!);

            result.UnknownKeys.Count.Should().Be( _testConfig.UnknownKeys );
            result.UnkeyedParameters.Count.Should().Be( _testConfig.UnkeyedParameters );

            foreach( var optConfig in _testConfig.OptionConfigurations )
            {
                optConfig.Option!.ValuesSatisfied.Should().Be( optConfig.ValuesSatisfied );
            }
        }

        protected void ValidateConfiguration<TParsed>()
            where TParsed : class, new()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJ4JCommandLine( _options!, _testConfig!.CommandLine, _allocator! );
            var config = configBuilder.Build();

            TParsed? parsed = null;

            if( _testConfig.OptionConfigurations.Any( x => x.ParsingWillFail ) )
            {
                var exception = Assert.Throws<InvalidOperationException>( () => config.Get<TParsed>() );
                return;
            }
            
            parsed = config.Get<TParsed>();

            if( _testConfig.OptionConfigurations.TrueForAll( x => !x.ValuesSatisfied )
                && _testConfig.OptionConfigurations.All( x => x.Style != OptionStyle.Switch ) )
            {
                parsed.Should().BeNull();
                return;
            }

            foreach( var optConfig in _testConfig.OptionConfigurations )
            {
                GetPropertyValue<TParsed>( parsed, optConfig.ContextKey, out var result, out var resultType )
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