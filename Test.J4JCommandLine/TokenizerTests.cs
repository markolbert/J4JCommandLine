using System;
using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests;

public class TokenizerTests : TestBase
{
    [ Theory ]
    [ MemberData( nameof( TestDataSource.GetTokenizerData ), MemberType = typeof( TestDataSource ) ) ]
    public void Simple( TokenizerConfig config )
    {
        var (_, parser) = GetOptionBuilderAndParser( config.OperatingSystem,
                                                     config.CommandLine,
                                                     new ConsolidateQuotedText(
                                                         StringComparison.OrdinalIgnoreCase ),
                                                     new MergeSequentialSeparators() );

        var tokens = parser.Tokenizer.Tokenize( config.CommandLine );
        tokens.Count.Should().Be( config.Data.Count );

        for( var idx = 0; idx < tokens.Count; idx++ )
        {
            var token = tokens[ idx ];
            token.Should().NotBeNull();

            token.Type.Should().Be( config.Data[ idx ].Type );
            token.Text.Should().Be( config.Data[ idx ].Text );
        }
    }

    [ Theory ]
    [ MemberData( nameof( TestDataSource.GetEmbeddedPropertyData ), MemberType = typeof( TestDataSource ) ) ]
    public void Embedded( TestConfig config )
    {
        var (optionBuilder, parser) = GetOptionBuilderAndParser(config.OperatingSystem,
                                                                config.CommandLine,
                                                                new ConsolidateQuotedText(
                                                                    StringComparison.OrdinalIgnoreCase),
                                                                new MergeSequentialSeparators());

        optionBuilder.Bind<EmbeddedTarget, bool>( x => x.Target1.ASwitch, config );
        optionBuilder.Bind<EmbeddedTarget, string>( x => x.Target1.ASingleValue, config );
        optionBuilder.Bind<EmbeddedTarget, TestEnum>( x => x.Target1.AnEnumValue, config );
        optionBuilder.Bind<EmbeddedTarget, TestFlagEnum>( x => x.Target1.AFlagEnumValue, config );
        optionBuilder.Bind<EmbeddedTarget, List<string>>( x => x.Target1.ACollection, config );

        ValidateTokenizing( config, parser );
    }

    [ Theory ]
    [ MemberData( nameof( TestDataSource.GetEmbeddedPropertyData ), MemberType = typeof( TestDataSource ) ) ]
    public void EmbeddedNoSetters( TestConfig config )
    {
        var (optionBuilder, parser) = GetOptionBuilderAndParser(config.OperatingSystem, config.CommandLine);

        optionBuilder.Bind<EmbeddedTargetNoSetter, bool>( x => x.Target1.ASwitch, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, string>( x => x.Target1.ASingleValue, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, TestEnum>( x => x.Target1.AnEnumValue, config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, TestFlagEnum>( x => x.Target1.AFlagEnumValue,
                                                    config );
        optionBuilder.Bind<EmbeddedTargetNoSetter, List<string>>( x => x.Target1.ACollection, config );

        ValidateTokenizing( config, parser );
    }
}
