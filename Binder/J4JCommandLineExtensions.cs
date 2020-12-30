using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            string cmdLine,
            IOptionCollection options )
        {
            builder.Add( new J4JCommandLineSource( options, cmdLine ) );

            return builder;
        }
    }
}