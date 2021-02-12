#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class ParserTests : BaseTest
    {
        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetTokenizerData), MemberType = typeof(TestDataSource) ) ]
        public void Tokenizer( TokenizerConfig config )
        {
            var tokenizer = new Tokenizer(
                AvailableTokens.GetDefault( CommandLineStyle.Windows, LoggerFactory ),
                LoggerFactory,
                new ConsolidateQuotedText( StringComparison.OrdinalIgnoreCase, LoggerFactory() ),
                new MergeSequentialSeparators() );

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
            Initialize( config );

            var parser = new Parser( Options );

            parser.Options.CreateOptionsFromContextKeys( config.OptionConfigurations );

            parser.Parse( config.CommandLine ).Should().BeTrue();

            ValidateConfiguration<BasicTarget>();
        }
    }
}