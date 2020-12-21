using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            string cmdLine,
            OptionCollection options )
        {
            builder.Add( new J4JCommandLineSource( options, cmdLine ) );
            
            return builder;
        }
    }
}