using System;
using Binder.Tests;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class SingleValueConfigTests : BaseTest
    {
        [ Theory ]
        [ InlineData( "ASingleValue", "x", "-x", null, false ) ]
        [ InlineData( "ASingleValue", "x", "-z", null, false ) ]
        [ InlineData( "ASingleValue", "x", "-x expected", "expected", false ) ]
        [ InlineData( "ASingleValue", "x", "-z expected", null, false ) ]
        [ InlineData( "ASingleValue", "x", "-x expected excess", "expected", false ) ]
        [ InlineData( "ASingleValue", "x", "-z expected excess", null, false ) ]
        public void ContextDefinitionText(
            string contextKey,
            string cmdLineKey,
            string cmdLine,
            string? parsedValue,
            bool throws )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( OptionStyle.SingleValued );

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

            tgtValue.Should().Be( parsedValue );
        }

        [Theory]
        [InlineData("AnEnumValue", "x", "-x EnumValue1", TestEnum.EnumValue1, false)]
        [InlineData("AnEnumValue", "x", "-x EnumValue2", TestEnum.EnumValue2, false)]
        [InlineData("AnEnumValue", "x", "-x EnumValue3", null, true)]
        [InlineData("AnEnumValue", "x", "-x enumvalue1", TestEnum.EnumValue1, false)]
        //[ InlineData( "AFlagEnumValue", OptionStyle.Collection, "x", "-x EnumValue1", TestFlagEnum.EnumValue1, false ) ]
        //[ InlineData( "AFlagEnumValue", OptionStyle.Collection, "x", "-x EnumValue2", TestFlagEnum.EnumValue2, false ) ]
        //[ InlineData( "AFlagEnumValue", OptionStyle.Collection, "x", "-x EnumValue1 EnumValue2",
        //    TestFlagEnum.EnumValue1 | TestFlagEnum.EnumValue2, false ) ]
        public void ContextDefinitionEnum(
            string contextKey,
            string cmdLineKey,
            string cmdLine,
            TestEnum? parsedValue,
            bool throws)
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add(contextKey);

            option.AddCommandLineKey(cmdLineKey)
                .SetStyle(OptionStyle.SingleValued);

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

            var tgtProp = typeof(ConfigTarget).GetProperty(contextKey);
            tgtProp.Should().NotBeNull();

            var tgtValue = tgtProp!.GetValue(result);

            tgtValue.Should().Be(parsedValue);
        }

        [ Theory ]
        [ InlineData( "AFlagEnumValue", "x", "-x EnumValue1", TestFlagEnum.EnumValue1, false ) ]
        [ InlineData( "AFlagEnumValue", "x", "-x EnumValue2", TestFlagEnum.EnumValue2, false ) ]
        [ InlineData( "AFlagEnumValue", "x", "-x EnumValue1 EnumValue2",
            TestFlagEnum.EnumValue1 | TestFlagEnum.EnumValue2, false ) ]
        public void ContextDefinitionFlagEnum(
            string contextKey,
            string cmdLineKey,
            string cmdLine,
            TestFlagEnum? parsedValue,
            bool throws )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( OptionStyle.ConcatenatedSingleValue );

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

            tgtValue.Should().Be( parsedValue );
        }

        [Theory]
        [InlineData(true, "x", "-x", null, false)]
        [InlineData(true, "x", "-z", null, false)]
        [InlineData(true, "x", "-x expected", "expected", false)]
        [InlineData(true, "x", "-z expected", null, false)]
        [InlineData(true, "x", "-x expected excess", "expected", false)]
        [InlineData(true, "x", "-z expected excess", null, false)]
        public void TypeBoundText(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            string? parsedValue,
            bool throws )
        {
            var options = CompositionRoot.Default.GetTypeBoundOptions<ConfigTarget>();

            options.Bind( x => x.ASingleValue, out var option )
                .Should()
                .Be( shouldBind );

            if (!shouldBind)
                return;

            option!.AddCommandLineKey( cmdLineKey );

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

            result.ASingleValue.Should().Be(parsedValue);
        }

        [Theory]
        [InlineData(true, "x", "-x", null, false)]
        [InlineData(true, "x", "-z", null, false)]
        [InlineData(true, "x", "-x EnumValue1", TestEnum.EnumValue1, false)]
        [InlineData(true, "x", "-x enumvalue1", TestEnum.EnumValue1, false)]
        [InlineData(true, "x", "-z EnumValue1", null, false)]
        [InlineData(true, "x", "-x EnumValue1 excess", TestEnum.EnumValue1, false)]
        [InlineData(true, "x", "-z EnumValue1 EnumValue2", null, false)]
        public void TypeBoundEnum(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            TestEnum? parsedValue,
            bool throws)
        {
            var options = CompositionRoot.Default.GetTypeBoundOptions<ConfigTarget>();

            options.Bind(x => x.AnEnumValue, out var option)
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

            result.AnEnumValue.Should().Be(parsedValue);
        }

        [Theory]
        [InlineData(true, "x", "-x", null, false)]
        [InlineData(true, "x", "-z", null, false)]
        [InlineData(true, "x", "-x EnumValue1", TestFlagEnum.EnumValue1, false)]
        [InlineData(true, "x", "-x enumvalue1", TestFlagEnum.EnumValue1, false)]
        [InlineData(true, "x", "-x enumvalue1 enumvalue2", TestFlagEnum.EnumValue1 | TestFlagEnum.EnumValue2, false)]
        [InlineData(true, "x", "-x enumvalue1 EnumValue2", TestFlagEnum.EnumValue1 | TestFlagEnum.EnumValue2, false)]
        [InlineData(true, "x", "-z EnumValue1", null, false)]
        [InlineData(true, "x", "-x EnumValue1 EnumValue2", TestFlagEnum.EnumValue1 | TestFlagEnum.EnumValue2, false)]
        [InlineData(true, "x", "-z EnumValue1 EnumValue2", null, false)]
        public void TypeBoundFlagEnum(
            bool shouldBind,
            string cmdLineKey,
            string cmdLine,
            TestFlagEnum? parsedValue,
            bool throws)
        {
            var options = CompositionRoot.Default.GetTypeBoundOptions<ConfigTarget>();

            options.Bind(x => x.AFlagEnumValue, out var option)
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

            result.AFlagEnumValue.Should().Be(parsedValue);
        }
    }
}
