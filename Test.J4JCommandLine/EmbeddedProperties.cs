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

using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class EmbeddedProperties : Validator
    {
        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource) ) ]
        public void Allocations( TestConfig testConfig )
        {
            var parser = CompositionRoot.Default.GetParser(CommandLineStyle.Windows);
            parser.Should().NotBeNull();

            Bind<EmbeddedTarget, bool>( parser!, x => x.Target1.ASwitch, testConfig );
            Bind<EmbeddedTarget, string>(parser!, x => x.Target1.ASingleValue, testConfig );
            Bind<EmbeddedTarget, TestEnum>(parser!, x => x.Target1.AnEnumValue, testConfig);
            Bind<EmbeddedTarget, TestFlagEnum>(parser!, x => x.Target1.AFlagEnumValue, testConfig);
            Bind<EmbeddedTarget, List<string>>(parser!, x => x.Target1.ACollection, testConfig);

            ValidateTokenizing( parser!, testConfig );
        }

        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource) ) ]
        public void Parsing( TestConfig testConfig )
        {
            var configParser = CompositionRoot.Default
                .GetConfigurationAndParser( CommandLineStyle.Windows, testConfig.CommandLine );

            configParser.parser.Should().NotBeNull();

            Bind<EmbeddedTarget, bool>( configParser.parser!, x => x.Target1.ASwitch, testConfig );
            Bind<EmbeddedTarget, string>( configParser.parser!, x => x.Target1.ASingleValue, testConfig );
            Bind<EmbeddedTarget, TestEnum>( configParser.parser!, x => x.Target1.AnEnumValue, testConfig );
            Bind<EmbeddedTarget, TestFlagEnum>( configParser.parser!, x => x.Target1.AFlagEnumValue, testConfig );
            Bind<EmbeddedTarget, List<string>>( configParser.parser!, x => x.Target1.ACollection, testConfig );

            configParser.parser!.Options.FinishConfiguration();
            configParser.parser.Options.Count.Should().BeGreaterThan(0);

            ValidateConfiguration<EmbeddedTarget>( configParser.configRoot, testConfig );
        }
    }
}