using System;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class SwitchPropertyTest
    {
        [Theory]
        [InlineData("-x 32", MappingResults.Success, true, new int[] { 32 })]
        [InlineData("-x", MappingResults.Success, true, new int[] { })]
        public void root_properties(
            string cmdLine,
            MappingResults result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind(x => x.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ProcessTest(cmdLine, target, option, unkeyed, result, () => target.Value, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", MappingResults.Success, true, new int[] { 32 })]
        [InlineData("-x", MappingResults.Success, true, new int[] { })]
        public void parameterless_properties(
            string cmdLine,
            MappingResults result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ProcessTest(cmdLine, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", MappingResults.Success, true, new int[] { 32 })]
        [InlineData("-x", MappingResults.Success, true, new int[] { })]
        public void parametered_properties(
            string cmdLine,
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

            ProcessTest(cmdLine, target, option, unkeyed, result, () => target.Value.TestProperties, optValue,
                unkeyedValues);
        }

        private void ProcessTest<T>(
            string cmdLine,
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
            unkeyed.Should().BeAssignableTo<MappableOption>();

            var parseResults = target.Parse(new string[] { cmdLine });
            parseResults.Should().Be(desiredParseResults);

            results().Switch.Should().Be(optValue);
            results().Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }
    }
}