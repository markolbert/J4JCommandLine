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
            _cmdLineParser = ServiceProvider.Instance.GetRequiredService<ICommandLineParser>();
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
        [ InlineData( "-x hello", new string[] { "hello" }, new string[] { } ) ]
        [ InlineData( "-x hello goodbye", new string[] { "hello" }, new string[] { "goodbye" } ) ]
        [ InlineData( "-x", new string[] { }, new string[] { } ) ]
        [ InlineData( "-x:abc", new string[] { "abc" }, new string[] { } ) ]
        [ InlineData( "-x:\"abc\"", new string[] { "\"abc\"" }, new string[] { } ) ]
        public void parse_one_item( string cmdLine, string[] optValue, string[] unkeyedValue )
        {
            var parsed = _cmdLineParser.Parse( cmdLine );

            parsed.Count.Should().Be( 1 );
            parsed.Contains( "x" ).Should().BeTrue();

            parsed[ "x" ].Parameters.Should().BeEquivalentTo( optValue );
            parsed.Unkeyed.Parameters.Should().BeEquivalentTo( unkeyedValue );
        }

        [ Theory ]
        [ InlineData( "-x hello -y goodbye", new string[] { "x", "y" }, new string[] { "hello" },
            new string[] { "goodbye" }, new string[] { } ) ]
        [ InlineData( "-x hello -y", new string[] { "x", "y" }, new string[] { "hello" }, new string[] { },
            new string[] { } ) ]
        [ InlineData( "-x hello goodbye -y", new string[] { "x", "y" },
            new string[] { "hello" }, new string[] { }, new string[] { "goodbye" } ) ]
        [ InlineData( "-x hello -x goodbye", new string[] { "x" }, new string[] { "hello", "goodbye" },
            new string[] { }, new string[] { } ) ]
        public void parse_two_items( string cmdLine, string[] keys, string[] results1, string[] results2,
            string[] unkeyedResults )
        {
            var results = new string[][] { results1, results2 };

            var parsed = _cmdLineParser.Parse( cmdLine );

            parsed.Count.Should().Be( keys.Length );

            for( var idx = 0; idx < keys.Length; idx++ )
            {
                parsed.Contains( keys[ idx ] ).Should().BeTrue();
                parsed[ keys[ idx ] ].Parameters.Should().BeEquivalentTo( results[ idx ] );
            }

            parsed.Unkeyed.Parameters.Should().BeEquivalentTo(unkeyedResults);
        }
    }
}
