using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class ParsingTests
    {
        private readonly ICommandLineParser _cmdLineParser;
        private readonly CommandLineErrors _errors;

        public ParsingTests()
        {
            _cmdLineParser = TestServiceProvider.Instance.GetRequiredService<ICommandLineParser>();
            _errors = new CommandLineErrors(StringComparison.OrdinalIgnoreCase);

            if (!_cmdLineParser.Initialize(
                StringComparison.OrdinalIgnoreCase,
                _errors,
                new string[] { "-", "--", "/" },
                new string[] { ":" },
                new char[] { '\'', '"' }))
                throw new ApplicationException($"{nameof(CommandLineParser)} initialization failed");
        }

        [ Theory ]
        [ InlineData( "x", new string[] { "-x" }, new string[] { "true" } ) ]
        [ InlineData( "x", new string[] { "-x 7" }, new string[] { "7" } ) ]
        [ InlineData( "x", new string[] { "-x -7" }, new string[] { "-7" } ) ]
        [ InlineData( "", new string[] { "value" }, null ) ]
        [ InlineData( "x", new string[] { "-x:abc" }, new string[] { "abc" } ) ]
        [ InlineData( "x", new string[] { "-x:\"abc\"" }, new string[] { "\"abc\"" } ) ]
        public void Single_parameter( string key, string[] input, string[] output )
        {
            var numResults = string.IsNullOrEmpty( key ) ? 0 : 1;

            var results = _cmdLineParser.Parse( input );

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
        [InlineData(new string[] {"x", "y", "z"}, "-x -y 7 -z -12", new int[] {1,1,1}, new string[] { "true", "7", "-12" })]
        [InlineData(new string[] { "x", "y", "z" }, "-x -y 7 8 -z -12", new int[] { 1, 2, 1 }, new string[] { "true", "7", "8", "-12" })]
        public void Command_line_string(string[] keys, string cmdLine, int[] rowLengths, string[] output)
        {
            if( keys.Length != rowLengths.Length)
                throw new ArgumentException("Mismatch between keys and output row lengths");

            var results = _cmdLineParser.Parse(cmdLine);

            results.Count.Should().Be(keys.Length);

            for( var idx = 0; idx < results.Count; idx++ )
            {
                results[idx].Key.Should().Be(keys[idx]);
                results[ idx ].NumParameters.Should().Be( rowLengths[ idx ] );

                var rowLength = rowLengths[ idx ];
                var rowOffset = rowLengths.Where( ( rl, i ) => i < idx ).Select( rl => rl ).Sum();
                var rowOutput = output[ rowOffset..( rowOffset + rowLength ) ];

                results[ idx ].Parameters.Should().BeEquivalentTo( rowOutput );
            }
        }
    }
}
