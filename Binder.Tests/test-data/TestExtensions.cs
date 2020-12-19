using System.Collections.Generic;
using J4JSoftware.CommandLine;

namespace J4JSoftware.Binder.Tests
{
    public static class TestExtensions
    {
        public static void CreateOptionsFromContextKeys( 
            this OptionCollection options,
            IEnumerable<OptionConfig> optConfigs )
        {
            foreach( var optConfig in optConfigs )
            {
                options.CreateOptionFromContextKey( optConfig );
            }
        }

        public static OptionCollection CreateOptionFromContextKey( this OptionCollection options, OptionConfig optConfig )
        {
            var option = options.Add( optConfig.ContextPath )
                .AddCommandLineKey( optConfig.CommandLineKey )
                .SetStyle( optConfig.Style );

            if( optConfig.Required ) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return options;
        }
    }
}