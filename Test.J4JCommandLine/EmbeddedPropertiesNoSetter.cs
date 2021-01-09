using System.Collections.Generic;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class EmbeddedPropertiesNoSetter : BaseTest
    {
        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource) ) ]
        public void Allocations( TestConfig config )
        {
            Initialize( config );

            Bind<EmbeddedTargetNoSetter, bool>( x => x.Target1.ASwitch );
            Bind<EmbeddedTargetNoSetter, string>( x => x.Target1.ASingleValue );
            Bind<EmbeddedTargetNoSetter, TestEnum>( x => x.Target1.AnEnumValue );
            Bind<EmbeddedTargetNoSetter, TestFlagEnum>( x => x.Target1.AFlagEnumValue );
            Bind<EmbeddedTargetNoSetter, List<string>>( x => x.Target1.ACollection );

            ValidateTokenizing();
        }

        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource) ) ]
        public void Parsing( TestConfig config )
        {
            Initialize( config );

            Bind<EmbeddedTargetNoSetter, bool>( x => x.Target1.ASwitch );
            Bind<EmbeddedTargetNoSetter, string>( x => x.Target1.ASingleValue );
            Bind<EmbeddedTargetNoSetter, TestEnum>( x => x.Target1.AnEnumValue );
            Bind<EmbeddedTargetNoSetter, TestFlagEnum>( x => x.Target1.AFlagEnumValue );
            Bind<EmbeddedTargetNoSetter, List<string>>( x => x.Target1.ACollection );

            ValidateConfiguration<EmbeddedTargetNoSetter>();
        }
    }
}