using System;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class SimplePropertyTest
    {
        [Theory]
        [InlineData("-z 32", true, MappingResults.MissingRequired, -1, new int[] { })]
        [InlineData("-x 32", true, MappingResults.Success, 32, new int[] { })]
        [InlineData("-z 32", false, MappingResults.Success, -1, new int[] { })]
        public void root_properties(
            string cmdLine,
            bool required,
            MappingResults result,
            int optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind(x => x.IntProperty, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ProcessTest( cmdLine, required, target, option, unkeyed, result, () => target.Value, optValue,
                unkeyedValues );
        }

        [Theory]
        [InlineData("-z 32", true, MappingResults.MissingRequired, -1, new int[] { })]
        [InlineData("-x 32", true, MappingResults.Success, 32, new int[] { })]
        [InlineData("-z 32", false, MappingResults.Success, -1, new int[] { })]
        public void parameterless_properties(
            string cmdLine,
            bool required,
            MappingResults result,
            int optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.IntProperty, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ProcessTest(cmdLine, required, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-z 32", true, MappingResults.MissingRequired, -1, new int[] { })]
        [InlineData("-x 32", true, MappingResults.Success, 32, new int[] { })]
        [InlineData("-z 32", false, MappingResults.Success, -1, new int[] { })]
        public void parametered_properties(
            string cmdLine,
            bool required,
            MappingResults result,
            int optValue,
            int[] unkeyedValues)
        {
            var target =
                ServiceProvider.GetBindingTarget<ParameteredConstructorParent>( 
                    false,
                    new ParameteredConstructorParent( 52 ) );

            var option = target!.Bind(x => x.TestProperties.IntProperty, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ProcessTest(cmdLine, required, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        private void ProcessTest<T>(
            string cmdLine, 
            bool required, 
            BindingTarget<T> target, 
            Option option, 
            Option unkeyed,
            MappingResults desiredParseResults,
            Func<TestProperties> results,
            int optValue,
            int[] unkeyedValues )
            where T : class
        {
            target.Should().NotBeNull();

            option.Should().BeAssignableTo<MappableOption>();
            option.SetDefaultValue(-1);

            unkeyed.Should().BeAssignableTo<MappableOption>();

            if (required)
                option.Required();

            var parseResults = target.Parse(new string[] { cmdLine });
            parseResults.Should().Be( desiredParseResults );

            results().Unkeyed.Should().BeEmpty();
            results().IntProperty.Should().Be(optValue);
            results().Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }
    }
}