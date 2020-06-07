using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace J4JCommandLine.Tests
{
    public class RootLevelTests
    {
        protected class RootProperties
        {
            public int IntProperty { get; set; }
            public List<int> IntList { get; set; }
            public int[] IntArray { get; set; }
        }

        private readonly BindingTargetBuilder _builder;

        public RootLevelTests()
        {
            _builder = TestServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            _builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");
        }

        [Theory]
        [InlineData("z", "32", true, MappingResults.MissingRequired, -1)]
        [InlineData("x", "32", true, MappingResults.Success, 32)]
        [InlineData("z", "32", false, MappingResults.Success, -1)]
        public void Simple_property_single(
            string key,
            string arg,
            bool required,
            MappingResults result,
            int desiredValue)
        {
            _builder.Build<RootProperties>(null, out var target, out var _);

            target.Should().NotBeNull();

            var option = target!.Bind(x => x.IntProperty, "x");

            option.SetDefaultValue( -1 );

            if (required)
                option.Required();

            var parseResult = target.Parse(new string[] { $"-{key}", arg });

            parseResult.Should().Be(result);

            var boundValue = target.Value.IntProperty;

            boundValue.Should().Be(Convert.ToInt32(desiredValue));
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

            _builder.Build<RootProperties>(null, out var target, out var _);

            target.Should().NotBeNull();

            var option = target!.Bind(x => x.IntArray, "x");

            if (required)
                option.Required();

            var cmdLineArgs = new List<string> { $"-{key}" };
            cmdLineArgs.AddRange(args);

            var parseResult = target.Parse(cmdLineArgs.ToArray());

            parseResult.Should().Be(result);

            var boundValue = target.Value.IntArray;

            boundValue.Should().BeEquivalentTo(desired);
        }

        [Theory]
        [InlineData("z", new string[] { "32", "33" }, true, MappingResults.MissingRequired, null)]
        [InlineData("x", new string[] { "32", "33" }, true, MappingResults.Success, new int[] { 32, 33 })]
        [InlineData("z", new string[] { "32", "33" }, false, MappingResults.Success, null)]
        public void Simple_property_list(
            string key,
            string[] args,
            bool required,
            MappingResults result,
            int[]? desiredValues)
        {
            var desired = desiredValues == null ? new List<int>() : new List<int>(desiredValues);

            _builder.Build<RootProperties>(null, out var target, out var _);

            target.Should().NotBeNull();

            var option = target!.Bind(x => x.IntList, "x");

            if (required)
                option.Required();

            var cmdLineArgs = new List<string> { $"-{key}" };
            cmdLineArgs.AddRange(args);

            var parseResult = target.Parse(cmdLineArgs.ToArray());

            parseResult.Should().Be(result);

            var boundValue = target.Value.IntList;

            boundValue.Should().BeEquivalentTo(desired);
        }

        [Theory]
        [InlineData(new string[] { "32" }, 0, Int32.MaxValue, MappingResults.Success)]
        [InlineData(new string[] { "32" }, 2, Int32.MaxValue, MappingResults.TooFewParameters)]
        [InlineData(new string[] { "32" }, 0, 0, MappingResults.TooManyParameters)]
        public void Num_parameters_list(
            string[] rawArgs,
            int minArgs,
            int maxArgs,
            MappingResults result)
        {
            _builder.Build<RootProperties>(null, out var target, out var _);

            target.Should().NotBeNull();

            var option = target!.Bind(x => x.IntList, "x");
            option.Should().BeAssignableTo<TargetedOption>();

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

            target.Value.IntList.Should().NotBeNull();
            target.Value.IntList.Count.Should().Be(expectedValues.Count);
            target.Value.IntList.Should().BeEquivalentTo(expectedValues);
        }

        [Theory]
        [InlineData(new string[] { "32" }, 0, Int32.MaxValue, MappingResults.Success)]
        [InlineData(new string[] { "32" }, 2, Int32.MaxValue, MappingResults.TooFewParameters)]
        [InlineData(new string[] { "32" }, 0, 0, MappingResults.TooManyParameters)]
        public void Num_parameters_array(
            string[] rawArgs,
            int minArgs,
            int maxArgs,
            MappingResults result)
        {
            _builder.Build<RootProperties>(null, out var target, out var _);

            target.Should().NotBeNull();

            var option = target!.Bind(x => x.IntArray, "x");
            option.Should().BeAssignableTo<TargetedOption>();

            option.ArgumentCount(minArgs, maxArgs);

            var args = rawArgs.ToList();
            args.Insert(0, "-x");

            var parseResult = target.Parse(args.ToArray());

            //var consoleText = _consoleWriter.ToString();

            parseResult.Should().Be(result);

            var expectedValues = new List<int>();
            var lowerLimit = minArgs > rawArgs.Length ? minArgs : 0;
            var upperLimit = maxArgs > rawArgs.Length ? rawArgs.Length : maxArgs;

            for (var idx = lowerLimit; idx < upperLimit; idx++)
            {
                expectedValues.Add(Convert.ToInt32(rawArgs[idx]));
            }

            target.Value.IntArray.Should().NotBeNull();
            target.Value.IntArray.Length.Should().Be(expectedValues.Count);
            target.Value.IntArray.Should().BeEquivalentTo(expectedValues.ToArray());
        }
    }
}
