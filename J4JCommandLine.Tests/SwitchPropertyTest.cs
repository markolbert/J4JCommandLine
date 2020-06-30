using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class SwitchPropertyTest
    {
        [Theory]
        [InlineData("-x 32", true, true, new int[] { 32 })]
        [InlineData("-x", true, true, new int[] { })]
        public void root_properties(
            string cmdLine,
            bool result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind(x => x.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ValidateSetup( target, option, unkeyed );

            target.Parse( new string[] { cmdLine } ).Should().Be( result );

            target.Value.Switch.Should().Be(optValue);
            target.Value.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", true, true, new int[] { 32 })]
        [InlineData("-x", true, true, new int[] { })]
        public void parameterless_properties(
            string cmdLine,
            bool result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ValidateSetup(target, option, unkeyed);

            target.Parse(new string[] { cmdLine }).Should().Be(result);

            target.Value.TestProperties.Switch.Should().Be(optValue);
            target.Value.TestProperties.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        [Theory]
        [InlineData("-x 32", true, true, new int[] { 32 })]
        [InlineData("-x", true, true, new int[] { })]
        public void parametered_properties(
            string cmdLine,
            bool result,
            bool optValue,
            int[] unkeyedValues)
        {
            var target =
                ServiceProvider.GetBindingTarget<ParameteredConstructorParent>(
                    false,
                    new ParameteredConstructorParent(52));

            var option = target!.Bind(x => x.TestProperties.Switch, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ValidateSetup(target, option, unkeyed);

            target.Parse(new string[] { cmdLine }).Should().Be(result);

            target.Value.TestProperties.Switch.Should().Be(optValue);
            target.Value.TestProperties.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        private void ValidateSetup<T>( BindingTarget<T> target, Option option, Option unkeyed )
            where T : class
        {
            target.Should().NotBeNull();
            option.Should().BeAssignableTo<MappableOption>();
            unkeyed.Should().BeAssignableTo<MappableOption>();
        }
    }
}