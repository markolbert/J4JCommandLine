using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
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

        private readonly StringWriter _consoleWriter = new StringWriter();
        private readonly TextConverter _textConv = new TextConverter();

        public HelpErrorTests()
        {
            Console.SetOut( _consoleWriter );
        }

        [ Theory ]
        [ InlineData( "h", MappingResults.HelpRequested ) ]
        public void Trigger_help(
            string key,
            MappingResults result )
        {
            var target = TestServiceProvider.Instance.GetRequiredService<IBindingTarget<RootProperties>>();

            var parseResult = target.Parse( new string[] { $"-{key}" } );

            var consoleText = _consoleWriter.ToString();

            parseResult.Should().Be( result );
        }
    }
}
