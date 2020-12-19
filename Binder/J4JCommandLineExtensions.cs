using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static IConfigurationBuilder AddJ4JCommandLine( 
            this IConfigurationBuilder builder, 
            OptionCollection options,
            string cmdLine, 
            IAllocator allocator)
        {
            builder.Add( new J4JCommandLineSource( options, cmdLine, allocator ) );
            
            return builder;
        }
    }
}