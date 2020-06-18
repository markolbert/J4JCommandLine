using System;
using System.Collections.Generic;
using Alba.CsConsoleFormat;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class TextConverterProperties
    {
        public bool Boolean { get; set; }
        public DateTime DateTime { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public int Integer { get; set; }
        public long Long { get; set; }
        public string Text { get; set; }
        public List<string> Unkeyed { get; set; }
    }

    public class TextConverterTests
    {
        [Theory]
        [InlineData("-x", true, true, new string[] { })]
        [InlineData("-z", true, false, new string[] { })]
        [InlineData("-x abc", true, true, new string[] { "abc" })]
        public void boolean(
            string cmdLine,
            bool parseResult,
            bool optValue,
            string[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TextConverterProperties>(false);
            var option = target!.Bind(x => x.Boolean, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            target.Should().NotBeNull();
            option.Should().BeAssignableTo<MappableOption>();
            unkeyed.Should().BeAssignableTo<MappableOption>();

            target.Parse(new string[] { cmdLine }).Should().Be(parseResult);

            target.Value.Boolean.Should().Be(optValue);
            target.Value.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", true, 32, new string[] { })]
        [InlineData("-x abc", false, 0, new string[] { })]
        [InlineData("-x 32 abc", true, 32, new string[] { "abc" })]
        public void integer(
            string cmdLine,
            bool parseResult,
            int optValue,
            string[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TextConverterProperties>(false);
            var option = target!.Bind(x => x.Integer, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            target.Should().NotBeNull();
            option.Should().BeAssignableTo<MappableOption>();
            unkeyed.Should().BeAssignableTo<MappableOption>();

            target.Parse( new string[] { cmdLine } ).Should().Be( parseResult );

            target.Value.Integer.Should().Be( optValue );
            target.Value.Unkeyed.Should().BeEquivalentTo( unkeyedValues );
        }
    }
}