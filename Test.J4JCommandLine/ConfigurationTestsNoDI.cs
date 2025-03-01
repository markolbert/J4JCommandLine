using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace J4JSoftware.Binder.Tests;

public class ConfigurationTestsNoDi : TestBase
{
    [ Theory ]
    [ MemberData( nameof( TestDataSource.GetEmbeddedPropertyData ), MemberType = typeof( TestDataSource ) ) ]
    public void EmbeddedConfiguration( TestConfig config )
    {
        var optionBuilder = GetOptionBuilder( config.OperatingSystem, config.CommandLine );

        optionBuilder.Bind<EmbeddedTarget, bool>( x => x.Target1.ASwitch, config );
        optionBuilder.Bind<EmbeddedTarget, string>( x => x.Target1.ASingleValue, config );
        optionBuilder.Bind<EmbeddedTarget, TestEnum>( x => x.Target1.AnEnumValue, config );
        optionBuilder.Bind<EmbeddedTarget, TestFlagEnum>( x => x.Target1.AFlagEnumValue, config );
        optionBuilder.Bind<EmbeddedTarget, List<string>>( x => x.Target1.ACollection, config );

        optionBuilder.Options.Count.Should().Be( 5 );

        ValidateConfiguration<EmbeddedTarget>( config, optionBuilder );
    }

    [ Theory ]
    [ MemberData( nameof( TestDataSource.GetEmbeddedPropertyData ), MemberType = typeof( TestDataSource ) ) ]
    public void EmbeddedConfigurationNoSetter( TestConfig config )
    {
        var optionBuilder = GetOptionBuilder(config.OperatingSystem, config.CommandLine);

        optionBuilder.Bind<EmbeddedTargetNoSetter, bool>( x => x.Target1.ASwitch, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, string>( x => x.Target1.ASingleValue, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, TestEnum>( x => x.Target1.AnEnumValue, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, TestFlagEnum>( x => x.Target1.AFlagEnumValue, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, List<string>>( x => x.Target1.ACollection, config );

        optionBuilder.Options.Count.Should().BeGreaterThan( 0 );

        ValidateConfiguration<EmbeddedTargetNoSetter>( config, optionBuilder );
    }
}
