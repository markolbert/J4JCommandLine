﻿using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;

namespace J4JSoftware.Binder.Tests
{
    public static class TestExtensions
    {
        public static void CreateOptionsFromContextKeys(
            this IOptionCollection options,
            IEnumerable<OptionConfig> optConfigs )
        {
            foreach( var optConfig in optConfigs ) options.CreateOptionFromContextKey( optConfig );
        }

        private static IOptionCollection CreateOptionFromContextKey( this IOptionCollection options,
            OptionConfig optConfig )
        {
            var option = options.Add( optConfig.GetPropertyType(), optConfig.ContextPath );
            option.Should().NotBeNull();

            option!.AddCommandLineKey( optConfig.CommandLineKey )
                .SetStyle( optConfig.Style );

            if( optConfig.Required ) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return options;
        }
    }
}