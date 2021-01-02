using FluentAssertions;
using J4JSoftware.CommandLine.Deprecated;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class SimplePropertyTest
    {
        [Theory]
        [InlineData("-z 32", true, false, -1, new int[] { })]
        [InlineData("-x 32", true, true, 32, new int[] { })]
        [InlineData("-z 32", false, true, -1, new int[] { })]
        public void Root_properties(
            string cmdLine,
            bool required,
            bool result,
            int optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<TestProperties>(false);
            var option = target!.Bind( x => x.IntProperty, "x" );
            var unkeyed = target!.BindUnkeyed(x => x.Unkeyed);

            ValidateSetup(target, option, unkeyed, required);

            target.Parse( new string[] { cmdLine } ).Should().Be( result );
            target.Value.IntProperty.Should().Be( optValue );
            target.Value.Unkeyed.Should().BeEquivalentTo( unkeyedValues );
        }

        [Theory]
        [InlineData("-z 32", true, false, -1, new int[] { })]
        [InlineData("-x 32", true, true, 32, new int[] { })]
        [InlineData("-z 32", false, true, -1, new int[] { })]
        public void Parameterless_properties(
            string cmdLine,
            bool required,
            bool result,
            int optValue,
            int[] unkeyedValues)
        {
            var target = ServiceProvider.GetBindingTarget<ParameterlessConstructorParent>(false);
            var option = target!.Bind(x => x.TestProperties.IntProperty, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ValidateSetup(target, option, unkeyed, required);

            target.Parse(new string[] { cmdLine }).Should().Be(result);
            target.Value.TestProperties.IntProperty.Should().Be(optValue);
            target.Value.TestProperties.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        [Theory]
        [InlineData("-z 32", true, false, -1, new int[] { })]
        [InlineData("-x 32", true, true, 32, new int[] { })]
        [InlineData("-z 32", false, true, -1, new int[] { })]
        public void Parametered_properties(
            string cmdLine,
            bool required,
            bool result,
            int optValue,
            int[] unkeyedValues)
        {
            var target =
                ServiceProvider.GetBindingTarget<ParameteredConstructorParent>( 
                    false,
                    new ParameteredConstructorParent( 52 ) );

            var option = target!.Bind(x => x.TestProperties.IntProperty, "x");
            var unkeyed = target!.BindUnkeyed(x => x.TestProperties.Unkeyed);

            ValidateSetup(target, option, unkeyed, required);

            target.Parse(new string[] { cmdLine }).Should().Be(result);
            target.Value.TestProperties.IntProperty.Should().Be(optValue);
            target.Value.TestProperties.Unkeyed.Should().BeEquivalentTo(unkeyedValues);
        }

        private void ValidateSetup<T>( BindingTarget<T> target, Option option, Option unkeyed, bool required )
            where T : class
        {
            target.Should().NotBeNull();
            option.Should().BeAssignableTo<MappableOption>();
            unkeyed.Should().BeAssignableTo<MappableOption>();

            option.SetDefaultValue(-1);

            if( required )
                option.Required();
        }
    }
}