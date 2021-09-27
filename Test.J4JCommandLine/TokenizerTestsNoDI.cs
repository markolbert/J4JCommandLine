using System;
using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Serilog;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class TokenizerTestsNoDI : TestBaseNoDI
    {
        private IParser? _parser;

        [Theory]
        [MemberData(nameof(TestDataSource.GetTokenizerData), MemberType = typeof(TestDataSource))]
        public void Simple(TokenizerConfig config)
        {
            Initialize(config.OperatingSystem,
                new ConsolidateQuotedText(StringComparison.OrdinalIgnoreCase, Logger),
                new MergeSequentialSeparators());

            var tokens = _parser!.Tokenizer.Tokenize(config.CommandLine);
            tokens.Count.Should().Be(config.Data.Count);

            for (var idx = 0; idx < tokens.Count; idx++)
            {
                var token = tokens[idx];
                token.Should().NotBeNull();

                token!.Type.Should().Be(config.Data[idx].Type);
                token!.Text.Should().Be(config.Data[idx].Text);
            }
        }

        [Theory]
        [MemberData(nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource))]
        public void Embedded(TestConfig testConfig)
        {
            Initialize(testConfig.OperatingSystem);

            Bind<EmbeddedTarget, bool>(_parser!.Collection!, x => x.Target1.ASwitch, testConfig);
            Bind<EmbeddedTarget, string>(_parser!.Collection!, x => x.Target1.ASingleValue, testConfig);
            Bind<EmbeddedTarget, TestEnum>(_parser!.Collection!, x => x.Target1.AnEnumValue, testConfig);
            Bind<EmbeddedTarget, TestFlagEnum>(_parser!.Collection!, x => x.Target1.AFlagEnumValue, testConfig);
            Bind<EmbeddedTarget, List<string>>(_parser!.Collection!, x => x.Target1.ACollection, testConfig);

            ValidateTokenizing(testConfig);
        }

        [Theory]
        [MemberData(nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource))]
        public void EmbeddedNoSetters(TestConfig testConfig)
        {
            Initialize(testConfig.OperatingSystem);

            Bind<EmbeddedTargetNoSetter, bool>(_parser!.Collection!, x => x.Target1.ASwitch, testConfig);
            Bind<EmbeddedTargetNoSetter, string>(_parser!.Collection!, x => x.Target1.ASingleValue, testConfig);
            Bind<EmbeddedTargetNoSetter, TestEnum>(_parser!.Collection!, x => x.Target1.AnEnumValue, testConfig);
            Bind<EmbeddedTargetNoSetter, TestFlagEnum>(_parser!.Collection!, x => x.Target1.AFlagEnumValue, testConfig);
            Bind<EmbeddedTargetNoSetter, List<string>>(_parser!.Collection!, x => x.Target1.ACollection, testConfig);

            ValidateTokenizing(testConfig);
        }

        private void Initialize( string osName, 
            params ICleanupTokens[] cleanupTokens )
        {

            _parser = osName.Equals( "windows", StringComparison.OrdinalIgnoreCase )
                ? Parser.GetWindowsDefault( logger: Logger, cleanupProcessors: cleanupTokens )
                : Parser.GetLinuxDefault( logger: Logger, cleanupProcessors: cleanupTokens );

            _parser!.Collection.Should().NotBeNull();
        }

        private void ValidateTokenizing(TestConfig testConfig)
        {
            _parser.Should().NotBeNull();

            _parser!.Parse(testConfig.CommandLine)
                .Should()
                .BeTrue();

            _parser.Collection.UnknownKeys.Count.Should().Be(testConfig.UnknownKeys);
            _parser.Collection.SpuriousValues.Count.Should().Be(testConfig.UnkeyedValues);

            foreach (var optConfig in testConfig.OptionConfigurations)
            {
                optConfig.Option!
                    .ValuesSatisfied
                    .Should()
                    .Be(optConfig.ValuesSatisfied);
            }
        }
    }
}