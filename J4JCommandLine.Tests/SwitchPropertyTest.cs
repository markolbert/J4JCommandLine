using System;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class SwitchPropertyTest
    {
        [Theory]
        [InlineData("-x 32", true, MappingResults.Success, true, new int[] { 32 })]
        [InlineData("-x", true, MappingResults.Success, true, new int[] { })]
        [InlineData("-x", false, MappingResults.MissingParameter, false, new int[] { })]
        public void root_properties(
            string cmdLine,
            bool isSwitch,
            MappingResults result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind(x => x.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ProcessTest(cmdLine, isSwitch, target, option, unkeyed, result, () => target.Value, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", true, MappingResults.Success, true, new int[] { 32 })]
        [InlineData("-x", true, MappingResults.Success, true, new int[] { })]
        [InlineData("-x", false, MappingResults.MissingParameter, false, new int[] { })]
        public void parameterless_properties(
            string cmdLine,
            bool isSwitch,
            MappingResults result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ProcessTest(cmdLine, isSwitch, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", true, MappingResults.Success, true, new int[] { 32 })]
        [InlineData("-x", true, MappingResults.Success, true, new int[] { })]
        [InlineData("-x", false, MappingResults.MissingParameter, false, new int[] { })]
        public void parametered_properties(
            string cmdLine,
            bool isSwitch,
            MappingResults result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target =
                ServiceProvider.GetBindingTarget<ParameteredConstructorParent>(
                    false,
                    new ParameteredConstructorParent(52));

            var option = target!.Bind(x => x.TestProperties.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ProcessTest(cmdLine, isSwitch, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        private void ProcessTest<T>(
            string cmdLine,
            bool isSwitch,
            BindingTarget<T> target,
            Option option,
            Option unkeyed,
            MappingResults desiredParseResults,
            Func<TestProperties> results,
            bool optValue,
            int[] unkeyedValues)
            where T : class
        {
            target.Should().NotBeNull();

            option.Should().BeAssignableTo<MappableOption>();
            option.Switch(isSwitch);

            unkeyed.Should().BeAssignableTo<MappableOption>();

            var parseResults = target.Parse(new string[] { cmdLine });
            parseResults.Should().Be(desiredParseResults);

            results().Switch.Should().Be(optValue);
            results().Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }
    }
}