using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class EnumTests
    {
        [ Theory ]
        [ InlineData( "z", "B", true, false, PlainEnum.A ) ]
        [ InlineData( "x", "B", true, true, PlainEnum.B ) ]
        [ InlineData( "z", "B", false, true, PlainEnum.A ) ]
        public void Plain_enum(
            string key,
            string arg,
            bool required,
            bool result,
            PlainEnum desiredValue )
        {
            var target = ServiceProvider.GetBindingTarget<EnumProperties>( true );
            target.Should().NotBeNull();

            var option = target!.Bind( x => x.Plain, "x" );

            option.SetDefaultValue( PlainEnum.A );

            if( required )
                option.Required();

            var parseResult = target.Parse( new string[] { $"-{key}", arg } );

            parseResult.Should().Be( result );

            var boundValue = target.Value.Plain;

            boundValue.Should().Be( desiredValue );
        }

        [Theory]
        [InlineData("z", "A,B", true, false, FlagsEnum.A)]
        [InlineData("x", "A,B", true, true, FlagsEnum.A | FlagsEnum.B)]
        [InlineData("z", "A,B", false, true, FlagsEnum.A)]
        public void Flags_enum(
            string key,
            string arg,
            bool required,
            bool result,
            FlagsEnum desiredValue)
        {
            var target = ServiceProvider.GetBindingTarget<EnumProperties>(true);
            target.Should().NotBeNull();

            var option = target!.Bind(x => x.Flags, "x");

            option.SetDefaultValue(FlagsEnum.A);

            if (required)
                option.Required();

            var parseResult = target.Parse(new string[] { $"-{key}", arg });

            parseResult.Should().Be(result);

            var boundValue = target.Value.Flags;

            boundValue.Should().Be(desiredValue);
        }

        [Theory]
        [InlineData("z", "A", true, false, UnconvertibleEnum.A)]
        [InlineData("x", "B", true, false, UnconvertibleEnum.A)]
        [InlineData("z", "A", false, false, UnconvertibleEnum.A)]
        public void Unconvertible_enum(
            string key,
            string arg,
            bool required,
            bool result,
            UnconvertibleEnum desiredValue)
        {
            var target = ServiceProvider.GetBindingTarget<EnumProperties>(true);
            target.Should().NotBeNull();

            var option = target!.Bind(x => x.Unconvertible, "x");

            option.SetDefaultValue(UnconvertibleEnum.A);

            if (required)
                option.Required();

            var parseResult = target.Parse(new string[] { $"-{key}", arg });

            parseResult.Should().Be(result);

            var boundValue = target.Value.Unconvertible;

            boundValue.Should().Be(desiredValue);
        }
    }
}
