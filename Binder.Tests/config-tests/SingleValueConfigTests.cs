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
        [ InlineData( "ASingleValue", OptionStyle.SingleValued, "x", "-x", "", false ) ]
        [ InlineData( "ASingleValue", OptionStyle.SingleValued, "x", "-z", "", false ) ]
        [ InlineData( "ASingleValue", OptionStyle.SingleValued, "x", "-x expected", "expected", false ) ]
        [ InlineData( "ASingleValue", OptionStyle.SingleValued, "x", "-z expected", "", false ) ]
        [ InlineData( "ASingleValue", OptionStyle.SingleValued, "x", "-x expected excess", "expected", false ) ]
        [ InlineData( "ASingleValue", OptionStyle.SingleValued, "x", "-z expected excess", "", false ) ]
        [ InlineData( "AnEnumValue", OptionStyle.SingleValued, "x", "-x EnumValue1", TestEnum.EnumValue1, false ) ]
        [ InlineData( "AnEnumValue", OptionStyle.SingleValued, "x", "-x EnumValue2", TestEnum.EnumValue2, false ) ]
        [ InlineData( "AnEnumValue", OptionStyle.SingleValued, "x", "-x EnumValue3", null, true ) ]
        [ InlineData( "AnEnumValue", OptionStyle.SingleValued, "x", "-x enumvalue1", TestEnum.EnumValue1, false ) ]
        [ InlineData( "AFlagEnumValue", OptionStyle.Collection, "x", "-x EnumValue1", TestFlagEnum.EnumValue1, false ) ]
        [ InlineData( "AFlagEnumValue", OptionStyle.Collection, "x", "-x EnumValue2", TestFlagEnum.EnumValue2, false ) ]
        [ InlineData( "AFlagEnumValue", OptionStyle.Collection, "x", "-x EnumValue1 EnumValue2",
            TestFlagEnum.EnumValue1 | TestFlagEnum.EnumValue2, false ) ]
        public void ByContextDefinition(
            string contextKey,
            OptionStyle style,
            string cmdLineKey,
            string cmdLine,
            object parsedValue,
            bool throws )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( style );

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

            var tgtProp = typeof(ConfigTarget).GetProperty( contextKey );
            tgtProp.Should().NotBeNull();

            var tgtValue = tgtProp!.GetValue( result );

            tgtValue.Should().Be( parsedValue );
        }
    }
}
