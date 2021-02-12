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
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class EmbeddedProperties : BaseTest
    {
        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource) ) ]
        public void Allocations( TestConfig config )
        {
            Initialize( config );

            Bind<EmbeddedTarget, bool>( x => x.Target1.ASwitch );
            Bind<EmbeddedTarget, string>( x => x.Target1.ASingleValue );
            Bind<EmbeddedTarget, TestEnum>( x => x.Target1.AnEnumValue );
            Bind<EmbeddedTarget, TestFlagEnum>( x => x.Target1.AFlagEnumValue );
            Bind<EmbeddedTarget, List<string>>( x => x.Target1.ACollection );

            ValidateTokenizing();
        }

        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource) ) ]
        public void Parsing( TestConfig config )
        {
            Initialize( config );

            Bind<EmbeddedTarget, bool>( x => x.Target1.ASwitch );
            Bind<EmbeddedTarget, string>( x => x.Target1.ASingleValue );
            Bind<EmbeddedTarget, TestEnum>( x => x.Target1.AnEnumValue );
            Bind<EmbeddedTarget, TestFlagEnum>( x => x.Target1.AFlagEnumValue );
            Bind<EmbeddedTarget, List<string>>( x => x.Target1.ACollection );

            ValidateConfiguration<EmbeddedTarget>();
        }
    }
}