using Binder.Tests;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class SwitchTests : BaseTest
    {
        [Theory]
        [InlineData("ASwitch", "x", "-x", true, 0, 0)]
        [InlineData("ASwitch", "x", "-z", false, 1, 0)]
        [InlineData("ASwitch", "x", "-x excess", true, 0, 1)]
        [InlineData("ASwitch", "x", "-z excess", false, 1, 1)]
        public void ContextDefinition( 
            string contextKey, 
            string cmdLineKey, 
            string cmdLine, 
            bool valuesSatisfied,
            int unknownKeys,
            int unkeyedParams )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( OptionStyle.Switch );

            var allocator = CompositionRoot.Default.Allocator;

            var result = allocator.AllocateCommandLine( cmdLine, options );
            
            option.ValuesSatisfied.Should().Be( valuesSatisfied );

            result.UnknownKeys.Count.Should().Be( unknownKeys );
            result.UnkeyedParameters.Count.Should().Be( unkeyedParams );
        }
        
        [Theory]
        [InlineData(true, "x", "-x", true, 0, 0)]
        [InlineData(true, "x", "-z", false, 1, 0)]
        [InlineData(true, "x", "-x excess", true, 0, 1)]
        [InlineData(true, "x", "-z excess", false, 1, 1)]
        public void TypeBound(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            bool valuesSatisfied,
            int unknownKeys,
            int unkeyedParams)
        {
            var options = CompositionRoot.Default.GetTypeBoundOptions<ConfigTarget>();

            options.Bind( x => x.ASwitch, out var option)
                .Should()
                .Be( shouldBind );

            if( !shouldBind )
                return;

            option!.AddCommandLineKey(cmdLineKey);

            var allocator = CompositionRoot.Default.Allocator;

            var result = allocator.AllocateCommandLine(cmdLine, options);

            option!.ValuesSatisfied.Should().Be(valuesSatisfied);

            result.UnknownKeys.Count.Should().Be(unknownKeys);
            result.UnkeyedParameters.Count.Should().Be(unkeyedParams);
        }
    }
}
