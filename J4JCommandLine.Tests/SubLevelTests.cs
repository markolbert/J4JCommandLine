using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class SubLevelTests
    {
        protected class SimpleChildProperties
        {
            public int IntProperty { get; set; }
            public List<int> IntList { get; set; }
            public int[] IntArray { get; set; }
        }

        protected class ComplexChildProperties
        {
            public ComplexChildProperties( int someValue )
            {
            }

            public int IntProperty { get; set; }
            public List<int> IntList { get; set; }
            public int[] IntArray { get; set; }
        }

        protected class RootProperties
        {
            public SimpleChildProperties SimpleChildProperties { get; set; }
            public ComplexChildProperties ComplexChildProperties { get; set; }
        }

        private readonly BindingTargetBuilder _builder;

        public SubLevelTests()
        {
            _builder = TestServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            _builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");
        }

        [ Theory ]
        [ InlineData( "z", "32", true, MappingResults.MissingRequired, -1 ) ]
        [InlineData("x", "32", true, MappingResults.Success, 32)]
        [InlineData("z", "32", false, MappingResults.Success, -1)]
        public void Simple_property_single(
            string key,
            string arg,
            bool required,
            MappingResults result,
            int desiredValue )
        {
            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind( x => x.SimpleChildProperties.IntProperty, "x" );

            option.SetDefaultValue( -1 );

            if( required )
                option.Required();

            var parseResult = target.Parse( new string[] { $"-{key}", arg } );

            parseResult.Should().Be( result );

            var subProp = target.Value.SimpleChildProperties;
            var boundValue = target.Value.SimpleChildProperties.IntProperty;

            boundValue.Should().Be( Convert.ToInt32( desiredValue ) );
        }

        [Theory]
        [InlineData("z", new string[] { "32", "33" }, true, MappingResults.MissingRequired, null)]
        [InlineData("x", new string[] { "32", "33" }, true, MappingResults.Success, new int[] { 32, 33 })]
        [InlineData("z", new string[] { "32", "33" }, false, MappingResults.Success, null)]
        public void Simple_property_array(
            string key,
            string[] args,
            bool required,
            MappingResults result,
            int[]? desiredValues)
        {
            var desired = desiredValues == null ? new List<int>() : new List<int>(desiredValues);

            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind( x => x.SimpleChildProperties.IntArray, "x" );

            if (required)
                option.Required();

            option.ArgumentCount( desired.Count );

            var cmdLineArgs = new List<string> { $"-{key}" };
            cmdLineArgs.AddRange(args);

            var parseResult = target.Parse(cmdLineArgs.ToArray());

            parseResult.Should().Be(result);

            var subProp = target.Value.SimpleChildProperties;
            var boundValue = target.Value.SimpleChildProperties.IntArray;

            boundValue.Should().BeEquivalentTo(desired);
        }

        [Theory ]
        [ InlineData( "z", new string[] { "32", "33" }, true, MappingResults.MissingRequired, null)]
        [InlineData("x", new string[] { "32", "33" }, true, MappingResults.Success, new int[]{32,33})]
        [InlineData("z", new string[] { "32", "33" }, false, MappingResults.Success, null)]
        public void Simple_property_list(
            string key,
            string[] args,
            bool required,
            MappingResults result,
            int[]? desiredValues)
        {
            var desired = desiredValues == null ? new List<int>() : new List<int>( desiredValues );

            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind( x => x.SimpleChildProperties.IntList, "x" );

            if (required)
                option.Required();

            option.ArgumentCount( desired.Count );

            var cmdLineArgs = new List<string> { $"-{key}" };
            cmdLineArgs.AddRange( args );

            var parseResult = target.Parse( cmdLineArgs.ToArray() );

            parseResult.Should().Be(result);

            var subProp = target.Value.SimpleChildProperties;
            var boundValue = target.Value.SimpleChildProperties.IntList;

            boundValue.Should().BeEquivalentTo( desired );
        }

        [Theory]
        [InlineData("x", "32")]
        public void Complex_property_single(
            string key,
            string arg)
        {
            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind(x => x.ComplexChildProperties.IntProperty, "x" );

            var parseResult = target.Parse(new string[] { $"-{key}", arg });

            parseResult.Should().Be( MappingResults.NotDefinedOrCreatable );

            var boundValue = target.Value.ComplexChildProperties;

            boundValue.Should().BeNull();
        }

        [ Theory ]
        [ InlineData( "x", "32" ) ]
        public void Complex_properties_collection(
            string key,
            string arg )
        {
            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind( x => x.ComplexChildProperties.IntList, "x" );

            var parseResult = target.Parse( new string[] { $"-{key}", arg } );

            parseResult.Should().Be( MappingResults.NotDefinedOrCreatable );

            var boundValue = target.Value.ComplexChildProperties;

            boundValue.Should().BeNull();
        }

        [Theory]
        [InlineData(new string[]{ "32"}, 0, Int32.MaxValue, MappingResults.Success)]
        [InlineData(new string[] { "32" }, 2, Int32.MaxValue, MappingResults.TooFewParameters)]
        [InlineData(new string[] { "32" }, 0, 0, MappingResults.Success)]
        public void Num_parameters_list(
            string[] rawArgs,
            int minArgs,
            int maxArgs,
            MappingResults result)
        {
            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind( x => x.SimpleChildProperties.IntList, "x" );
            option.Should().BeAssignableTo<MappableOption>();

            option.ArgumentCount( minArgs, maxArgs );

            var args = rawArgs.ToList();
            args.Insert(0, "-x");

            var parseResult = target.Parse( args.ToArray() );

            parseResult.Should().Be(result);

            var expectedValues = new List<int>();
            var lowerLimit = minArgs > rawArgs.Length ? minArgs : 0;
            var upperLimit = maxArgs > rawArgs.Length ? rawArgs.Length : maxArgs;

            for( var idx = lowerLimit; idx < upperLimit; idx++ )
            {
                expectedValues.Add( Convert.ToInt32( rawArgs[ idx ] ) );
            }

            target.Value.SimpleChildProperties.IntList.Should().NotBeNull();
            target.Value.SimpleChildProperties.IntList.Count.Should().Be( expectedValues.Count );
            target.Value.SimpleChildProperties.IntList.Should().BeEquivalentTo( expectedValues );
        }

        [Theory]
        [InlineData(new string[] { "32" }, 0, Int32.MaxValue, MappingResults.Success)]
        [InlineData(new string[] { "32" }, 2, Int32.MaxValue, MappingResults.TooFewParameters)]
        [InlineData(new string[] { "32" }, 0, 0, MappingResults.Success)]
        public void Num_parameters_array(
            string[] rawArgs,
            int minArgs,
            int maxArgs,
            MappingResults result)
        {
            _builder.Build<RootProperties>(null, out var target);

            target.Should().NotBeNull();

            var option = target!.Bind( x => x.SimpleChildProperties.IntArray, "x" );
            option.Should().BeAssignableTo<MappableOption>();

            option.ArgumentCount(minArgs, maxArgs);

            var args = rawArgs.ToList();
            args.Insert(0, "-x");

            var parseResult = target.Parse(args.ToArray());

            parseResult.Should().Be(result);

            var expectedValues = new List<int>();
            var lowerLimit = minArgs > rawArgs.Length ? minArgs : 0;
            var upperLimit = maxArgs > rawArgs.Length ? rawArgs.Length : maxArgs;

            for (var idx = lowerLimit; idx < upperLimit; idx++)
            {
                expectedValues.Add(Convert.ToInt32(rawArgs[idx]));
            }

            target.Value.SimpleChildProperties.IntArray.Should().NotBeNull();
            target.Value.SimpleChildProperties.IntArray.Length.Should().Be(expectedValues.Count);
            target.Value.SimpleChildProperties.IntArray.Should().BeEquivalentTo(expectedValues.ToArray());
        }
    }
}
