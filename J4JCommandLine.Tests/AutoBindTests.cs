using System.Linq;
using FluentAssertions;
using J4JSoftware.CommandLine.Deprecated;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class AutoBindTests
    {
        [Theory]
        [InlineData("-i 32 -t junk", true, 32, "junk", new string[] { })]
        public void Working(
            string cmdLine,
            bool result,
            int intValue,
            string textValue,
            string[] unkeyedValues)
        {
            var builder = ServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-")
                .Quotes('\'', '"')
                .HelpKeys("h")
                .ProgramName($"{nameof(AutoBindTests.Working)}")
                .Description("a test program for exercising J4JCommandLine")
                .IgnoreUnprocessedUnkeyedParameters( false );

            var target = builder.AutoBind<AutoBindProperties>();
            target.Should().NotBeNull();

            target!.Options.Count().Should().Be( 3 );

            target.Parse( new string[] { cmdLine } ).Should().Be( result );
            target.Value.IntProperty.Should().Be( intValue );
            target.Value.TextProperty.Should().Be( textValue );
            target.Value.Unkeyed.Should().BeEquivalentTo( unkeyedValues );
        }

        [Theory]
        [InlineData("-i 32 -t junk")]
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
        public void Broken( string cmdLine )
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
        {
            var builder = ServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-")
                .Quotes('\'', '"')
                .HelpKeys("h")
                .ProgramName($"{nameof(AutoBindTests.Working)}")
                .Description("a test program for exercising J4JCommandLine")
                .IgnoreUnprocessedUnkeyedParameters(false);

            var target = builder.AutoBind<AutoBindPropertiesBroken>();
            target.Should().BeNull();
        }
    }
}