using System;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class ListPropertyTest
    {
        [Theory]
        [InlineData("-z 32 33", true, MappingResult.MissingRequired, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 33", true, MappingResult.Success, new int[] { 32 }, new int[] { 33 })]
        [InlineData("-z 32 33", false, MappingResult.Success, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 -x 33", true, MappingResult.Success, new int[] { 32, 33 }, new int[] { })]
        public void root_properties(
            string cmdLine,
            bool required,
            MappingResult result,
            int[] optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind(x => x.IntList, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ProcessTest(cmdLine, required, target, option, unkeyed, result, () => target.Value, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-z 32 33", true, MappingResult.MissingRequired, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 33", true, MappingResult.Success, new int[] { 32 }, new int[] { 33 })]
        [InlineData("-z 32 33", false, MappingResult.Success, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 -x 33", true, MappingResult.Success, new int[] { 32, 33 }, new int[] { })]
        public void parameterless_properties(
            string cmdLine,
            bool required,
            MappingResult result,
            int[] optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.IntList, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ProcessTest(cmdLine, required, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-z 32 33", true, MappingResult.MissingRequired, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 33", true, MappingResult.Success, new int[] { 32 }, new int[] { 33 })]
        [InlineData("-z 32 33", false, MappingResult.Success, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 -x 33", true, MappingResult.Success, new int[] { 32, 33 }, new int[] { })]
        public void parametered_properties(
            string cmdLine,
            bool required,
            MappingResult result,
            int[] optValue,
            int[] unkeyedValues)
        {
            var target =
                ServiceProvider.GetBindingTarget<ParameteredConstructorParent>(
                    false,
                    new ParameteredConstructorParent(52));

            var option = target!.Bind(x => x.TestProperties.IntList, "x");
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
            MappingResult desiredParseResult,
            Func<TestProperties> results,
            int[] optValue,
            int[] unkeyedValues)
            where T : class
        {
            target.Should().NotBeNull();

            option.Should().BeAssignableTo<MappableOption>();
            unkeyed.Should().BeAssignableTo<MappableOption>();

            if (required)
                option.Required();

            var parseResults = target.Parse(new string[] { cmdLine });
            parseResults.Should().Be(desiredParseResult);

            results().IntList.Should().BeEquivalentTo(optValue);
            results().Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }
    }
}