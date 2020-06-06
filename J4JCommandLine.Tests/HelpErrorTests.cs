using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class HelpErrorTests
    {
        protected class RootProperties
        {
            public int IntProperty { get; set; }
            public List<int> IntList { get; set; }
            public int[] IntArray { get; set; }
        }

        private readonly BindingTargetBuilder _builder;

        public HelpErrorTests()
        {
            _builder = TestServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            _builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");
        }

        [ Theory ]
        [ InlineData( "h", MappingResults.HelpRequested ) ]
        public void Trigger_help(
            string key,
            MappingResults result )
        {
            _builder.Build<RootProperties>(null, out var target, out var _);

            target.Should().NotBeNull();

            var parseResult = target!.Parse( new string[] { $"-{key}" } );

            parseResult.Should().Be( result );
        }

        [Theory]
        [InlineData(MappingResults.UnspecifiedFailure)]
        public void No_help_keys( MappingResults result )
        {
            var builder = TestServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");

            builder.Build<RootProperties>(null, out var target, out var errors);

            target.Should().BeNull();
            errors.Count.Should().BeGreaterThan( 0 );
        }
    }
}
