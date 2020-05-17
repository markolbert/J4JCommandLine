using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class CommandLineParsing
    {
        private readonly ICommandLineTextParser _cmdLineParser;
        private readonly IOptionCollection _options;

        public CommandLineParsing()
        {
            var parseConfig = new ParsingConfiguration();
            parseConfig.AddPrefixes("-", "--");
            parseConfig.AddTextDelimiters("\"", "'");
            parseConfig.AddValueEnclosers(":", "=", " ");

            _cmdLineParser = new CommandLineTextParser( parseConfig );

            _options = new OptionCollection( parseConfig );
        }

        [ Theory ]
        [ InlineData( new string[] { "-x", "hello" }, "x", new string[] { "hello" } ) ]
        [ InlineData( new string[] { "-x", "hello", "goodbye" }, "x", new string[] { "hello", "goodbye" } ) ]
        [ InlineData( new string[] { "-x" }, "x", new string[] { "true" } ) ]
        [ InlineData( new string[] { "-x:abc" }, "x", new string[] { "abc" } ) ]
        [ InlineData( new string[] { "-x:\"abc\"" }, "x", new string[] { "abc" } ) ]
        public void parse_one_item( string[] toParse, string key, string[] args )
        {
            var parsed = _cmdLineParser.Parse( toParse );

            parsed.Count.Should().Be( 1 );
            parsed.Contains( key ).Should().BeTrue();

            parsed[ key ].Parameters.Should().BeEquivalentTo( args );
        }

        [ Theory ]
        [ InlineData( new string[] { "-x", "hello", "-y", "goodbye" }, new string[] { "x", "y" },
            new string[] { "hello" }, new string[] { "goodbye" } ) ]
        [InlineData(new string[] { "-x", "hello", "-y" }, new string[] { "x", "y" },
            new string[] { "hello" }, new string[] { "true" })]
        [InlineData(new string[] { "-x", "hello", "goodbye", "-y" }, new string[] { "x", "y" },
            new string[] { "hello", "goodbye" }, new string[] { "true" })]
        [InlineData(new string[] { "-x", "hello", "-x", "goodbye" }, new string[] { "x" },
            new string[] { "hello", "goodbye" }, new string[] {})]
        public void parse_two_items( string[] toParse, string[] keys, string[] results1, string[] results2 )
        {
            var results = new string[][] { results1, results2 };

            var parsed = _cmdLineParser.Parse( toParse );

            parsed.Count.Should().Be( keys.Length );

            for( var idx = 0; idx < keys.Length; idx++ )
            {
                parsed.Contains( keys[ idx ] ).Should().BeTrue();
                parsed[ keys[ idx ] ].Parameters.Should().BeEquivalentTo( results[ idx ] );
            }
        }
    }
}
