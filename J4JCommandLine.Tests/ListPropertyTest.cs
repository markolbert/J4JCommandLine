using FluentAssertions;
using J4JSoftware.CommandLine.Deprecated;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class ListPropertyTest
    {
        [Theory]
        [InlineData("-z 32 33", true, false, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 33", true, true, new int[] { 32 }, new int[] { 33 })]
        [InlineData("-z 32 33", false, true, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 -x 33", true, true, new int[] { 32, 33 }, new int[] { })]
        public void Root_properties(
            string cmdLine,
            bool required,
            bool result,
            int[] optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind(x => x.IntList, "x");
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ValidateSetup(target, option, unkeyed, required);

            target.Parse( new string[] { cmdLine } ).Should().Be( result );

            target.Value.IntList.Should().BeEquivalentTo(optValue);
            target.Value.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        [Theory]
        [InlineData("-z 32 33", true, false, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 33", true, true, new int[] { 32 }, new int[] { 33 })]
        [InlineData("-z 32 33", false, true, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 -x 33", true, true, new int[] { 32, 33 }, new int[] { })]
        public void Parameterless_properties(
            string cmdLine,
            bool required,
            bool result,
            int[] optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.IntList, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ValidateSetup(target, option, unkeyed, required);

            target.Parse(new string[] { cmdLine }).Should().Be(result);

            target.Value.TestProperties.IntList.Should().BeEquivalentTo(optValue);
            target.Value.TestProperties.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        [Theory]
        [InlineData("-z 32 33", true, false, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 33", true, true, new int[] { 32 }, new int[] { 33 })]
        [InlineData("-z 32 33", false, true, new int[] { }, new int[] { 33 })]
        [InlineData("-x 32 -x 33", true, true, new int[] { 32, 33 }, new int[] { })]
        public void Parametered_properties(
            string cmdLine,
            bool required,
            bool result,
            int[] optValue,
            int[] unkeyedValues)
        {
            var target =
                ServiceProvider.GetBindingTarget<ParameteredConstructorParent>(
                    false,
                    new ParameteredConstructorParent(52));

            var option = target!.Bind(x => x.TestProperties.IntList, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ValidateSetup(target, option, unkeyed, required);

            target.Parse(new string[] { cmdLine }).Should().Be(result);

            target.Value.TestProperties.IntList.Should().BeEquivalentTo(optValue);
            target.Value.TestProperties.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        private void ValidateSetup<T>( BindingTarget<T> target, Option option, Option unkeyed, bool required )
            where T : class
        {
            target.Should().NotBeNull();
            option.Should().BeAssignableTo<MappableOption>();
            unkeyed.Should().BeAssignableTo<MappableOption>();

            if (required)
                option.Required();
        }
    }
}