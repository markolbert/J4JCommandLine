using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class EmbeddedProperties : BaseTest
    {
        [Theory]
        [MemberData(nameof(TestDataSource.GetEmbeddedPropertyData), MemberType=typeof(TestDataSource))]
        public void Allocations( TestConfig config )
        {
            Initialize( config );

            Bind<EmbeddedTarget, bool>( x => x.Target1.ASwitch );
            Bind<EmbeddedTarget, string>(x => x.Target1.ASingleValue);
            Bind<EmbeddedTarget, TestEnum>(x => x.Target1.AnEnumValue);
            Bind<EmbeddedTarget, TestFlagEnum>(x => x.Target1.AFlagEnumValue);
            Bind<EmbeddedTarget, List<string>>(x => x.Target1.ACollection);

            ValidateAllocations();
        }

        [Theory]
        [MemberData(nameof(TestDataSource.GetEmbeddedPropertyData), MemberType = typeof(TestDataSource))]
        public void Parsing(TestConfig config)
        {
            Initialize(config);

            Bind<EmbeddedTarget, bool>(x => x.Target1.ASwitch);
            Bind<EmbeddedTarget, string>(x => x.Target1.ASingleValue);
            Bind<EmbeddedTarget, TestEnum>(x => x.Target1.AnEnumValue);
            Bind<EmbeddedTarget, TestFlagEnum>(x => x.Target1.AFlagEnumValue);
            Bind<EmbeddedTarget, List<string>>(x => x.Target1.ACollection);

            ValidateConfiguration<EmbeddedTarget>();
        }
    }
}