using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            string cmdLine,
            MasterTextCollection masterText,
            IAllocator allocator,
            CommandLineLogger logger,
            out OptionCollection options )
        {
            options = new OptionCollection( masterText, logger );
            
            builder.Add( new J4JCommandLineSource( options, cmdLine, allocator ) );
            
            return builder;
        }

        public static IConfigurationBuilder AddJ4JCommandLineWindows(
            this IConfigurationBuilder builder,
            string cmdLine,
            out OptionCollection options,
            out CommandLineLogger logger,
            IAllocator? allocator = null )
        {
            logger = new CommandLineLogger();
            
            allocator ??= new Allocator(
                new ElementTerminator( MasterTextCollection.WindowsDefault, logger ),
                new KeyPrefixer( MasterTextCollection.WindowsDefault, logger ),
                logger );
            
            builder.AddJ4JCommandLine( 
                cmdLine, 
                MasterTextCollection.WindowsDefault, 
                allocator,
                logger,
                out var innerOptions);

            options = innerOptions;

            return builder;
        }

        public static IConfigurationBuilder AddJ4JCommandLineWindows(
            this IConfigurationBuilder builder,
            string cmdLine,
            out IAllocator allocator,
            out OptionCollection options,
            out CommandLineLogger logger)
        {
            logger = new CommandLineLogger();

            allocator = new Allocator(
                new ElementTerminator(MasterTextCollection.WindowsDefault, logger),
                new KeyPrefixer(MasterTextCollection.WindowsDefault, logger),
                logger);

            builder.AddJ4JCommandLine(
                cmdLine,
                MasterTextCollection.WindowsDefault,
                allocator,
                logger,
                out var innerOptions);

            options = innerOptions;

            return builder;
        }

        public static IConfigurationBuilder AddJ4JCommandLineLinux(
            this IConfigurationBuilder builder,
            string cmdLine,
            out OptionCollection options,
            out CommandLineLogger logger,
            IAllocator? allocator = null)
        {
            logger = new CommandLineLogger();

            allocator ??= new Allocator(
                new ElementTerminator(MasterTextCollection.LinuxDefault, logger),
                new KeyPrefixer(MasterTextCollection.LinuxDefault, logger),
                logger);

            builder.AddJ4JCommandLine(
                cmdLine,
                MasterTextCollection.LinuxDefault,
                allocator,
                logger,
                out var innerOptions);

            options = innerOptions;

            return builder;
        }

        public static IConfigurationBuilder AddJ4JCommandLineLinux(
            this IConfigurationBuilder builder,
            string cmdLine,
            out IAllocator allocator,
            out OptionCollection options,
            out CommandLineLogger logger)
        {
            logger = new CommandLineLogger();

            allocator = new Allocator(
                new ElementTerminator(MasterTextCollection.LinuxDefault, logger),
                new KeyPrefixer(MasterTextCollection.LinuxDefault, logger),
                logger);

            builder.AddJ4JCommandLine(
                cmdLine,
                MasterTextCollection.LinuxDefault,
                allocator,
                logger,
                out var innerOptions);

            options = innerOptions;

            return builder;
        }
    }
}