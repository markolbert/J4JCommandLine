using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            string cmdLine,
            OptionCollection options,
            out CommandLineLogger logger,
            IAllocator? allocator = null )
        {
            var masterText = new MasterTextCollection();
            logger = new CommandLineLogger();
            
            allocator ??= new Allocator( new ElementTerminator( masterText, logger ),
                new KeyPrefixer( masterText, logger ), logger );

            builder.Add( new J4JCommandLineSource( options, cmdLine, allocator ) );
            
            return builder;
        }
    }
}