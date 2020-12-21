using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class MiscAllocation
    {
        [ Theory ]
        [ InlineData( "-x abc", "abc" ) ]
        [ InlineData( "-x \"abc\"", "abc" ) ]
        public void StringHandling( string cmdLine, string? result )
        {
            var options = new OptionCollection();

            var option = options.Bind<MiscTarget, string?>( x => x.AStringValue, "x" );
            option.Should().NotBeNull();
            options.Log.HasMessages().Should().BeFalse();

            var allocResult = options.Allocator.AllocateCommandLine( cmdLine, options );
            allocResult.UnknownKeys.Should().BeEmpty();
            allocResult.UnkeyedParameters.Should().BeEmpty();

            option!.CommandLineValues.Count.Should().Be( 1 );
            option.CommandLineValues[ 0 ].Should().Be( result );
        }
    }
}