using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class EnumTests
    {
        protected class RootProperties
        {
            public PlainEnum Plain { get; set; }
            public FlagsEnum Flags { get; set; }
            public UnconvertibleEnum Unconvertible { get; set; }
        }

        private readonly BindingTargetBuilder _builder;

        public EnumTests()
        {
            _builder = TestServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            _builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");
        }

        [ Theory ]
        [ InlineData( "z", "B", true, MappingResults.MissingRequired, PlainEnum.A ) ]
        [ InlineData( "x", "B", true, MappingResults.Success, PlainEnum.B ) ]
        [ InlineData( "z", "B", false, MappingResults.Success, PlainEnum.A ) ]
        public void Plain_enum(
            string key,
            string arg,
            bool required,
            MappingResults result,
            PlainEnum desiredValue )
        {
            _builder.Build<RootProperties>(null, out var target );

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
        [InlineData("z", "A,B", true, MappingResults.MissingRequired, FlagsEnum.A)]
        [InlineData("x", "A,B", true, MappingResults.Success, FlagsEnum.A | FlagsEnum.B)]
        [InlineData("z", "A,B", false, MappingResults.Success, FlagsEnum.A)]
        public void Flags_enum(
            string key,
            string arg,
            bool required,
            MappingResults result,
            FlagsEnum desiredValue)
        {
            _builder.Build<RootProperties>(null, out var target);

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
        [InlineData("z", "A", true, MappingResults.Unbound, UnconvertibleEnum.A)]
        [InlineData("x", "B", true, MappingResults.Unbound, UnconvertibleEnum.A)]
        [InlineData("z", "A", false, MappingResults.Unbound, UnconvertibleEnum.A)]
        public void Unconvertible_enum(
            string key,
            string arg,
            bool required,
            MappingResults result,
            UnconvertibleEnum desiredValue)
        {
            _builder.Build<RootProperties>(null, out var target);

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
