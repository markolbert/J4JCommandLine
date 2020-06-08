using System;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class CommandLineParsing
    {
        private readonly ICommandLineParser _cmdLineParser;
        private readonly CommandLineErrors _errors;

        public CommandLineParsing()
        {
            _cmdLineParser = TestServiceProvider.Instance.GetRequiredService<ICommandLineParser>();
            _errors = new CommandLineErrors( StringComparison.OrdinalIgnoreCase );

            var masterText = new MasterTextCollection(StringComparison.OrdinalIgnoreCase);

            masterText.AddRange(TextUsageType.HelpOptionKey, "h", "?");
            masterText.AddRange(TextUsageType.Prefix, "-", "--", "/");
            masterText.AddRange(TextUsageType.ValueEncloser, ":");
            masterText.AddRange( TextUsageType.Quote, "'", "\"" );

            if( !_cmdLineParser.Initialize(
                StringComparison.OrdinalIgnoreCase,
                _errors,
                masterText ) )
                throw new ApplicationException( $"{nameof(CommandLineParser)} initialization failed" );
        }

        [ Theory ]
        [ InlineData( new string[] { "-x", "hello" }, "x", new string[] { "hello" } ) ]
        [ InlineData( new string[] { "-x", "hello", "goodbye" }, "x", new string[] { "hello", "goodbye" } ) ]
        [ InlineData( new string[] { "-x" }, "x", new string[] { "true" } ) ]
        [ InlineData( new string[] { "-x:abc" }, "x", new string[] { "abc" } ) ]
        [ InlineData( new string[] { "-x:\"abc\"" }, "x", new string[] { "\"abc\"" } ) ]
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
