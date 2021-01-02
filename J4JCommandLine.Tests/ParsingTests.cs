using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using J4JSoftware.CommandLine.Deprecated;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class ParsingTests
    {
        private readonly IAllocator _cmdLineParser;
        private readonly CommandLineLogger _logger;

        public ParsingTests()
        {
            _cmdLineParser = ServiceProvider.Instance.GetRequiredService<IAllocator>();
            _logger = new CommandLineLogger(StringComparison.OrdinalIgnoreCase);

            var masterText = new MasterTextCollection(StringComparison.OrdinalIgnoreCase);

            masterText.AddRange(TextUsageType.HelpOptionKey, "h", "?");
            masterText.AddRange(TextUsageType.Prefix, "-", "--", "/");
            masterText.AddRange(TextUsageType.ValueEncloser, ":");
            masterText.AddRange(TextUsageType.Quote, "'", "\"");

            if (!_cmdLineParser.Initialize(
                StringComparison.OrdinalIgnoreCase,
                _logger,
                masterText))
                throw new ApplicationException($"{nameof(Allocator)} initialization failed");
        }

        [ Theory ]
        [ InlineData( "x", new string[] { "-x" }, new string[] { } ) ]
        [ InlineData( "x", new string[] { "-x 7" }, new string[] { "7" } ) ]
        [ InlineData( "x", new string[] { "-x -7" }, new string[] { "-7" } ) ]
        [ InlineData( "", new string[] { "value" }, null ) ]
        [ InlineData( "x", new string[] { "-x:abc" }, new string[] { "abc" } ) ]
        [ InlineData( "x", new string[] { "-x:\"abc\"" }, new string[] { "\"abc\"" } ) ]
        public void Single_parameter( string key, string[] input, string[] output )
        {
            var numResults = string.IsNullOrEmpty( key ) ? 0 : 1;

            var results = _cmdLineParser.AllocateCommandLine( input );

            results.Count.Should().Be( numResults );

            if( numResults > 0 )
            {
                results[ 0 ].NumParameters.Should().Be( output.Length );
                results[ 0 ].Key.Should().Be( key );
                results[ 0 ].Parameters.Should().BeEquivalentTo( new List<string>( output ) );
            }
            else results.Count.Should().Be( 0 );
        }

        [Theory]
        [InlineData("-x -y 7 -z -12", 3, new string[] { "7", "-12" }, new string[]{})]
        [InlineData("-x -y 7 8 -z -12", 3, new string[] { "7", "-12" }, new string[]{"8"})]
        [InlineData("abc -x -y 7 8 -z -12", 3, new string[] { "7", "-12" }, new string[] { "abc", "8" })]
        public void Command_line_string(string cmdLine, int numKeys, string[] keyedOutput, string[] unkeyedOutput )
        {
            var results = _cmdLineParser.AllocateCommandLine(cmdLine);

            results.Count.Should().Be(numKeys);

            var consolidatedOutput = results.SelectMany( r => r.Parameters ).ToArray();
            consolidatedOutput.Should().BeEquivalentTo( keyedOutput );

            results.Unkeyed.Parameters.Should().BeEquivalentTo(unkeyedOutput);
        }
    }
}
