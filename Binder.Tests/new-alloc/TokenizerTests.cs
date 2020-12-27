using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class TokenizerTests
    {
        [Theory]
        [MemberData(nameof(TestDataSource.GetTokenizerData), MemberType = typeof(TestDataSource))]
        public void Simple( TokenizerConfig config )
        {
            var cmdLogger = new CommandLineLogger();
            var tokenColl = TokenCollection.GetDefault( CommandLineStyle.Windows, cmdLogger );

            var tokens = tokenColl.Tokenize( config.CommandLine );

            tokens.Count.Should().Be( config.Data.Count + 1 );

            for( var idx = 0; idx < tokens.Count; idx++ )
            {
                if( idx < tokens.Count - 1 )
                {
                    tokens[ idx ].Type.Should().Be( config.Data[ idx ].Type );
                    tokens[ idx ].Text.Should().Be( config.Data[ idx ].Text );
                }
                else
                {
                    tokens[idx].Type.Should().Be(TokenType.EndOfInput);
                    tokens[idx].Text.Should().Be(string.Empty);
                }
            }
        }
    }
}
