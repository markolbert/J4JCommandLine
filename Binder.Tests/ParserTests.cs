using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class ParserTests : BaseTest
    {
        [Theory]
        [MemberData(nameof(TestDataSource.GetTokenizerData), MemberType = typeof(TestDataSource))]
        public void Tokenizer( TokenizerConfig config )
        {
            var tokenizer = new Tokenizer(
                AvailableTokens.GetDefault(CommandLineStyle.Windows, LoggerFactory), 
                LoggerFactory,
                new ConsolidateQuotedText(StringComparison.OrdinalIgnoreCase, LoggerFactory()),
                new MergeSequentialSeparators(LoggerFactory()) );

            var tokens = tokenizer.Tokenize( config.CommandLine );

            tokens.Count.Should().Be( config.Data.Count );

            for( var idx = 0; idx < tokens.Count; idx++ )
            {
                var token = tokens[ idx ];
                token.Should().NotBeNull();

                token!.Type.Should().Be( config.Data[ idx ].Type );
                token!.Text.Should().Be( config.Data[ idx ].Text );
            }
        }

        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetParserData), MemberType = typeof(TestDataSource) ) ]
        public void Parser( TestConfig config )
        {
            Initialize(config);

            var parser = new Parser( Options, LoggerFactory );

            parser.Options.CreateOptionsFromContextKeys( config.OptionConfigurations );

            parser.Parse( config.CommandLine ).Should().BeTrue();

            ValidateConfiguration<BasicTarget>();
        }
    }
}
