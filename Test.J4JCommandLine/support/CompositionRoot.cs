using System;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.Binder.Tests
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        static CompositionRoot()
        {
            Default = new CompositionRoot();

            Default.Initialize();
        }

        private CompositionRoot() 
            : base( "J4JSoftware", "BinderTests" )
        {
            UseConsoleLifetime = true;

            var loggerConfig = new J4JLoggerConfiguration()
            {
                SourceRootPath = "C:/Programming/J4JCommandLine"
            };

            loggerConfig.Channels.Add(new DebugConfig()  );

            StaticConfiguredLogging( loggerConfig );
        }

        public static CompositionRoot Default { get; }

        public Func<IJ4JLogger> LoggerFactory => () => Host!.Services.GetRequiredService<IJ4JLogger>();

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( "appConfig.json" );
        }
    }
}