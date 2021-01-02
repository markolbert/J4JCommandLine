using System;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.Binder.Tests
{
    public class CompositionRoot : J4JCompositionRoot
    {
        public static CompositionRoot Default { get; } = new();

        public Func<IJ4JLogger> LoggerFactory => () => Host!.Services.GetRequiredService<IJ4JLogger>();

        protected override void ConfigureHostBuilder( IHostBuilder hostBuilder )
        {
            hostBuilder.ConfigureAppConfiguration( ( hc, b ) =>
                b.SetBasePath( Environment.CurrentDirectory )
                    .AddJsonFile( "appConfig.json" )
            );

            hostBuilder.ConfigureContainer<ContainerBuilder>( ConfigureDependencyInjection );
        }

        private void ConfigureDependencyInjection( HostBuilderContext context, ContainerBuilder builder )
        {
            var factory = new ChannelFactory( context.Configuration );

            factory.AddChannel<DebugConfig>( "Channels:Debug" );

            builder.RegisterJ4JLogging<J4JLoggerConfiguration>( factory );
        }
    }
}