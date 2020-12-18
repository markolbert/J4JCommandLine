using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class EmbeddedTest : BaseTest
    {
        private OptionsBase? _options;
        private string? _parentContextKey;
        private IAllocator? _allocator;
        
        [Theory]
        [InlineData("Target1", "ASwitch", "x", OptionStyle.Switch, "-x", true, 0, 0 )]
        public void ContextDefinition( 
            string parentContextKey,
            string contextKey,
            string cmdLineKey,
            OptionStyle style,
            string cmdLine,
            bool valuesSatisfied,
            int unknownKeys,
            int unkeyedParams )
        {
            Initialize(parentContextKey);

            var option = AddOption( contextKey, cmdLineKey, style );

            var result = _allocator!.AllocateCommandLine(cmdLine, _options!);

            option.ValuesSatisfied.Should().Be(valuesSatisfied);

            result.UnknownKeys.Count.Should().Be(unknownKeys);
            result.UnkeyedParameters.Count.Should().Be(unkeyedParams);
        }

        [ Theory ]
        [ InlineData( true, "ASwitch", "x", OptionStyle.Switch, "-x", true, 0, 0 ) ]
        public void TypeBound(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            bool parsedValue,
            bool throws )
        {
            Initialize();

            options.Bind(x => x.ASwitch, out var option)
                .Should()
                .Be(shouldBind);

            if (!shouldBind)
                return;

            option!.AddCommandLineKey(cmdLineKey);

            var allocator = CompositionRoot.Default.GetAllocator();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJ4JCommandLine(options, cmdLine, allocator);
            var config = configBuilder.Build();

            if (throws)
            {
                var exception = Assert.Throws<InvalidOperationException>(() => config.Get<BasicTarget>());
                return;
            }

            var result = config.Get<BasicTarget>();

            result.ASwitch.Should().Be(parsedValue);
        }

        private void Initialize( string? parentContextKey = null )
        {
            _options = parentContextKey == null
                ? CompositionRoot.Default.GetTypeBoundOptions<EmbeddedTarget>()
                : CompositionRoot.Default.GetOptions();
            
            _parentContextKey = parentContextKey;
            _allocator = CompositionRoot.Default.GetAllocator();
        }

        private Option AddOption(string contextKey, string cmdLineKey, OptionStyle style )
        {
            var retVal = ((Options) _options!).Add( $"{_parentContextKey}:{contextKey}" );

            return retVal.AddCommandLineKey( cmdLineKey )
                .SetStyle( style );
        }
    }
}
