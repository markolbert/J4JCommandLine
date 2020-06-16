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

        [ Theory ]
        [ InlineData( "h", MappingResults.HelpRequested ) ]
        public void Trigger_help(
            string key,
            MappingResults result )
        {
            var target = ServiceProvider.GetBindingTarget<RootProperties>(true);
            target.Should().NotBeNull();

            var parseResult = target!.Parse( new string[] { $"-{key}" } );

            parseResult.Should().Be( result );
        }

        [Fact]
        public void No_help_keys()
        {
            var builder = ServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");

            var target = builder.Build<RootProperties>( null );

            target.Should().BeNull();
        }

        [Fact]
        public void Duplicate_help_keys()
        {
            var builder = ServiceProvider.Instance.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h","h","h")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{this.GetType()}");

            var target = builder.Build<RootProperties>(null);

            target.Should().NotBeNull();
        }
    }
}
