using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class MiscTests : BaseTest
    {
        [ Theory ]
        [ InlineData( CommandLineStyle.Linux, "-x abc", "abc" ) ]
        [ InlineData(CommandLineStyle.Linux, "-x \"abc\"", "abc" ) ]
        public void StringHandling( CommandLineStyle style, string cmdLine, params string[] result )
        {
            Initialize(style);

            var option = Options.Bind<MiscTarget, string?>( x => x.AStringValue, "x" );
            option.Should().NotBeNull();

            var parser = new Parser(Options, LoggerFactory);
            parser.Parse( cmdLine ).Should().BeTrue();

            Options.UnknownKeys.Should().BeEmpty();
            Options.UnkeyedValues.Should().BeEmpty();

            option!.Values.Count.Should().Be( result.Length );

            for( var idx = 0; idx < result.Length; idx++ )
            {
                option.Values[ idx ].Should().Be( result[ idx ] );
            }
        }
    }
}