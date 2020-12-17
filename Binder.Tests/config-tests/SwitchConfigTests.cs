using System;
using Binder.Tests;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class SwitchConfigTests : BaseTest
    {
        [Theory]
        [InlineData("ASwitch", "x", "-x", true)]
        [InlineData("ASwitch", "x", "-z", false)]
        [InlineData("ASwitch", "x", "-x excess", true)]
        [InlineData("ASwitch", "x", "-z excess", false)]
        public void ContextDefinition( 
            string contextKey, 
            string cmdLineKey, 
            string cmdLine, 
            bool parsedValue
            )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( OptionStyle.Switch );

            var allocator = CompositionRoot.Default.Allocator;

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJ4JCommandLine( options, cmdLine, allocator );
            var config = configBuilder.Build();

            var result = config.Get<ConfigTarget>();

            result.ASwitch.Should().Be( parsedValue );
        }

        [Theory]
        [InlineData(true, "x", "-x", true, false)]
        [InlineData(true, "x", "-z", false, false)]
        [InlineData(true, "x", "-x excess", true, false)]
        [InlineData(true, "x", "-z excess", false, false)]
        public void TypeBound(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            bool parsedValue,
            bool throws
        )
        {
            var options = CompositionRoot.Default.GetTypeBoundOptions<ConfigTarget>();

            options.Bind(x => x.ASwitch, out var option)
                .Should()
                .Be(shouldBind);

            if (!shouldBind)
                return;

            option!.AddCommandLineKey(cmdLineKey);

            var allocator = CompositionRoot.Default.Allocator;

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJ4JCommandLine(options, cmdLine, allocator);
            var config = configBuilder.Build();

            if (throws)
            {
                var exception = Assert.Throws<InvalidOperationException>(() => config.Get<ConfigTarget>());
                return;
            }

            var result = config.Get<ConfigTarget>();

            result.ASwitch.Should().Be(parsedValue);
        }
    }
}
