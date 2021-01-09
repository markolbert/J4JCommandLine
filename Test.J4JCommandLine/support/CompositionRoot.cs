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
            Default = new CompositionRoot
            {
                UseConsoleLifetime = true,
                CachedLoggerScope = CachedLoggerScope.SingleInstance,
                LoggingSectionKey = string.Empty
            };

            Default.ChannelInformation
                .AddChannel<DebugConfig>("Channels:Debug");

            Default.Initialize();
        }

        public static CompositionRoot Default { get; }

        public Func<IJ4JLogger> LoggerFactory => () => Host!.Services.GetRequiredService<IJ4JLogger>();

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( "appConfig.json" );
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            var channelInfo = new ChannelInformation()
                .AddChannel<DebugConfig>( "Channels:Debug" );

            var factory = new ChannelFactory( hbc.Configuration, channelInfo );
            
            builder.RegisterJ4JLogging<J4JLoggerConfiguration>( factory );
        }
    }
}