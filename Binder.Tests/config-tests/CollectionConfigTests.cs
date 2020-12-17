using System;
using System.Collections.Generic;
using System.Linq;
using Binder.Tests;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class CollectionConfigTests : BaseTest
    {
        [ Theory ]
        [ InlineData( "ACollection", "x", "-x", null, false ) ]
        [ InlineData( "ACollection", "x", "-z", null, false ) ]
        [ InlineData( "ACollection", "x", "-x expected", new string[] {"expected"}, false ) ]
        [ InlineData( "ACollection", "x", "-z expected", null, false ) ]
        [ InlineData( "ACollection", "x", "-x expected excess", new string[] {"expected", "excess"}, false ) ]
        [ InlineData( "ACollection", "x", "-z expected excess", null, false ) ]
        public void ContextDefinition(
            string contextKey,
            string cmdLineKey,
            string cmdLine,
            string[]? parsedValue,
            bool throws )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( OptionStyle.Collection );

            var configBuilder = new ConfigurationBuilder();
            var allocator = CompositionRoot.Default.Allocator;

            configBuilder.AddJ4JCommandLine( options, cmdLine, allocator );
            var config = configBuilder.Build();

            if( throws )
            {
                var exception = Assert.Throws<InvalidOperationException>( () => config.Get<ConfigTarget>() );
                return;
            }

            var result = config.Get<ConfigTarget>();

            if( parsedValue == null )
            {
                result.Should().BeNull();
                return;
            }

            var tgtProp = typeof(ConfigTarget).GetProperty( contextKey );
            tgtProp.Should().NotBeNull();

            var tgtValue = tgtProp!.GetValue( result );
            tgtValue.Should().NotBeNull();
            tgtValue.Should().BeAssignableTo<List<string>>();

            ( (List<string>) tgtValue! ).Should().BeEquivalentTo( parsedValue.ToList() );
        }

        [Theory]
        [InlineData(true, "x", "-x", null, false)]
        [InlineData(true, "x", "-z", null, false)]
        [InlineData(true, "x", "-x expected", new string[]{ "expected"}, false)]
        [InlineData(true, "x", "-z expected", null, false)]
        [InlineData(true, "x", "-x expected excess", new string[] {"expected", "excess"}, false)]
        [InlineData(true, "x", "-z expected excess", null, false)]
        public void TypeBound(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            string[]? parsedValue,
            bool throws)
        {
            var options = CompositionRoot.Default.GetTypeBoundOptions<ConfigTarget>();

            options.Bind(x => x.ACollection, out var option)
                .Should()
                .Be(shouldBind);

            if (!shouldBind)
                return;

            option!.AddCommandLineKey(cmdLineKey);

            var configBuilder = new ConfigurationBuilder();
            var allocator = CompositionRoot.Default.Allocator;

            configBuilder.AddJ4JCommandLine(options, cmdLine, allocator);
            var config = configBuilder.Build();

            if (throws)
            {
                var exception = Assert.Throws<InvalidOperationException>(() => config.Get<ConfigTarget>());
                return;
            }

            var result = config.Get<ConfigTarget>();

            if (parsedValue == null)
            {
                result.Should().BeNull();
                return;
            }

            result.ACollection.Should().BeEquivalentTo( parsedValue.ToList() );
        }
    }
}
