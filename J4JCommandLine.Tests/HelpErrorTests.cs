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

        private readonly TextConverter _textConv = new TextConverter();

        public HelpErrorTests()
        {
        }

        [ Theory ]
        [ InlineData( "h", MappingResults.HelpRequested ) ]
        public void Trigger_help(
            string key,
            MappingResults result )
        {
            var target = TestServiceProvider.Instance.GetRequiredService<IBindingTarget<RootProperties>>();

            var parseResult = target.Parse( new string[] { $"-{key}" } );

            parseResult.Should().Be( result );
        }
    }
}
