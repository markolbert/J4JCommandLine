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

using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    //[UseAutofacTestFramework]
    public class MiscTests : TestBase
    {
        [ Theory ]
        [ InlineData( CommandLineStyle.Linux, "-x abc", "abc" ) ]
        [ InlineData( CommandLineStyle.Linux, "-x \"abc\"", "abc" ) ]
        public void StringHandling( CommandLineStyle style, string cmdLine, params string[] result )
        {
            ParserFactory.Create(style, out var parser)
                .Should()
                .BeTrue();

            parser.Should().NotBeNull();

            var option = parser!.Options.Bind<MiscTarget, string?>( x => x.AStringValue, "x" );
            option.Should().NotBeNull();

            parser.Parse( cmdLine ).Should().BeTrue();

            parser.Options.UnknownKeys.Should().BeEmpty();
            parser.Options.UnkeyedValues.Should().BeEmpty();

            option!.Values.Count.Should().Be( result.Length );

            for( var idx = 0; idx < result.Length; idx++ )
            {
                option.Values[ idx ].Should().Be( result[ idx ] );
            }
        }
    }
}