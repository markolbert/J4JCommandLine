using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class ConfigurationTests : TestBase
    {
        private IOptionCollection? _options;
        private IConfigurationRoot? _configRoot;
        private CommandLineSource? _cmdLineSrc;

        [ Theory ]
        [ MemberData( nameof( TestDataSource.GetParserData ), MemberType = typeof( TestDataSource ) ) ]
        public void Simple( TestConfig testConfig )
        {
            CreateConfigurationRootAndParser( testConfig );

            CreateOptionsFromContextKeys(_options!, testConfig.OptionConfigurations);
            _options!.FinishConfiguration();

            _options.Count.Should().Be(testConfig.OptionConfigurations.Count);

            ValidateConfiguration<BasicTarget>( testConfig );
        }

        [Theory]
        [MemberData(nameof(TestDataSource.GetSinglePropertyData), MemberType = typeof(TestDataSource))]
        public void SingleProperties(TestConfig testConfig)
        {
            CreateConfigurationRootAndParser(testConfig);

            CreateOptionsFromContextKeys(_options!, testConfig.OptionConfigurations);
            _options!.FinishConfiguration();

            _options.Count.Should().Be(testConfig.OptionConfigurations.Count);

            ValidateConfiguration<BasicTarget>(testConfig);
        }

        [Theory]
        [MemberData(nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource))]
        public void EmbeddedConfiguration(TestConfig testConfig)
        {
            CreateConfigurationRootAndParser(testConfig);

            Bind<EmbeddedTarget, bool>(_options!, x => x.Target1.ASwitch, testConfig);
            Bind<EmbeddedTarget, string>(_options!, x => x.Target1.ASingleValue, testConfig);
            Bind<EmbeddedTarget, TestEnum>(_options!, x => x.Target1.AnEnumValue, testConfig);
            Bind<EmbeddedTarget, TestFlagEnum>(_options!, x => x.Target1.AFlagEnumValue, testConfig);
            Bind<EmbeddedTarget, List<string>>(_options!, x => x.Target1.ACollection, testConfig);

            _options!.FinishConfiguration();
            _options.Count.Should().Be(5);

            ValidateConfiguration<EmbeddedTarget>(testConfig);
        }

        [ Theory ]
        [ MemberData( nameof( TestDataSource.GetEmbeddedPropertyData ), MemberType = typeof( TestDataSource ) ) ]
        public void EmbeddedConfigurationNoSetter( TestConfig testConfig )
        {
            CreateConfigurationRootAndParser( testConfig );

            Bind<EmbeddedTargetNoSetter, bool>( _options!, x => x.Target1.ASwitch, testConfig );
            Bind<EmbeddedTargetNoSetter, string>( _options!, x => x.Target1.ASingleValue, testConfig );
            Bind<EmbeddedTargetNoSetter, TestEnum>( _options!, x => x.Target1.AnEnumValue, testConfig );
            Bind<EmbeddedTargetNoSetter, TestFlagEnum>( _options!, x => x.Target1.AFlagEnumValue, testConfig );
            Bind<EmbeddedTargetNoSetter, List<string>>( _options!, x => x.Target1.ACollection, testConfig );

            _options!.FinishConfiguration();
            _options.Count.Should().BeGreaterThan( 0 );

            ValidateConfiguration<EmbeddedTargetNoSetter>( testConfig );
        }

        private void CreateConfigurationRootAndParser( TestConfig testConfig )
        {
            var parser = testConfig.OperatingSystem.Equals( "windows", StringComparison.OrdinalIgnoreCase )
                ? Parser.GetWindowsDefault( Logger )
                : Parser.GetLinuxDefault( Logger );

            _options = parser.Options;

            _configRoot = new ConfigurationBuilder()
                .AddJ4JCommandLine(
                    parser,
                    out _cmdLineSrc, 
                    Logger )
                .Build();

            _options.Should().NotBeNull();
        }

        private void ValidateConfiguration<TParsed>( TestConfig testConfig )
            where TParsed: class, new()
        {
            //_parser.Should().NotBeNull();
            //_parser!.Parse(testConfig.CommandLine).Should().BeTrue();

            _cmdLineSrc.Should().NotBeNull();
            _cmdLineSrc!.SetCommandLine(testConfig.CommandLine);

            if ( testConfig!.OptionConfigurations.Any( x => x.ConversionWillFail ) )
            {
                var exception = Assert.Throws<InvalidOperationException>( _configRoot.Get<TParsed> );
                return;
            }

            var parsed = _configRoot.Get<TParsed>();

            if( testConfig.OptionConfigurations.TrueForAll( x => !x.ValuesSatisfied ) )
                return;

            parsed.Should().NotBeNull();

            foreach( var optConfig in testConfig.OptionConfigurations )
            {
                GetPropertyValue( parsed, optConfig.ContextPath, out var result, out var resultType )
                    .Should()
                    .BeTrue();

                if( optConfig.Style == OptionStyle.Collection )
                {
                    if( optConfig.CorrectTextArray.Count == 0 )
                        result.Should().BeNull();
                    else result.Should().BeEquivalentTo( optConfig.CorrectTextArray );
                }
                else
                {
                    if( optConfig.CorrectText == null )
                    {
                        if( optConfig.ValuesSatisfied )
                            result.Should().BeNull();
                    }
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
            where TParsed: class, new()
        {
            result = null;
            resultType = null;

            Type curType = typeof( TParsed );
            object? curValue = parsed;

            var keys = contextKey.Split( ":", StringSplitOptions.RemoveEmptyEntries );

            for( var idx = 0; idx < keys.Length; idx++ )
            {
                var curPropInfo = curType.GetProperty( keys[ idx ] );

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