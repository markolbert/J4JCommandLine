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
            OptionCollection options,
            CommandLineLogger logger )
        {
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

            var mt = MasterTextCollection.GetDefault( CommandLineStyle.Windows );
            
            allocator ??= new Allocator(
                new ElementTerminator( mt, logger ),
                new KeyPrefixer( mt, logger ),
                logger );

            options = new OptionCollection( mt, logger );

            builder.AddJ4JCommandLine(
                cmdLine,
                mt,
                allocator,
                options,
                logger );

            return builder;
        }

        public static IConfigurationBuilder AddJ4JCommandLineWindows(
            this IConfigurationBuilder builder,
            string cmdLine,
            out OptionCollection options,
            out IAllocator allocator,
            out CommandLineLogger logger )
        {
            logger = new CommandLineLogger();

            var mt = MasterTextCollection.GetDefault(CommandLineStyle.Windows);

            allocator = new Allocator(
                new ElementTerminator(mt, logger),
                new KeyPrefixer(mt, logger),
                logger);

            options = new OptionCollection(mt, logger);

            builder.AddJ4JCommandLine(
                cmdLine,
                mt,
                allocator,
                options,
                logger);

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

            var mt = MasterTextCollection.GetDefault( CommandLineStyle.Linux );

            allocator ??= new Allocator(
                new ElementTerminator(mt, logger),
                new KeyPrefixer(mt, logger),
                logger);

            options = new OptionCollection(mt, logger);

            builder.AddJ4JCommandLine(
                cmdLine,
                mt,
                allocator,
                options,
                logger);

            return builder;
        }

        public static IConfigurationBuilder AddJ4JCommandLineLinux(
            this IConfigurationBuilder builder,
            string cmdLine,
            out OptionCollection options,
            out IAllocator allocator,
            out CommandLineLogger logger)
        {
            logger = new CommandLineLogger();

            var mt = MasterTextCollection.GetDefault(CommandLineStyle.Linux);

            allocator = new Allocator(
                new ElementTerminator(mt, logger),
                new KeyPrefixer(mt, logger),
                logger);

            options = new OptionCollection(mt, logger);

            builder.AddJ4JCommandLine(
                cmdLine,
                mt,
                allocator,
                options,
                logger);

            return builder;
        }

    }
}