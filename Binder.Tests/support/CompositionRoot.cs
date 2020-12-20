using System;
using Autofac;
using J4JSoftware.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.Binder.Tests
{
    public class CompositionRoot : J4JCompositionRoot
    {
        private static readonly CompositionRoot _root = new CompositionRoot();

        public static CompositionRoot Default => _root;

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
            builder.RegisterJ4JLogging();

            builder.Register( c =>
                {
                    var retVal = context.Configuration
                        .GetSection("Logger"  )
                        .Get<J4JLoggerConfiguration<LogChannelsConfig>>();

                    return retVal;
                } )
                .As<IJ4JLoggerConfiguration>();
        }

        public IJ4JLogger Logger => Host!.Services.GetRequiredService<IJ4JLogger>();
    }
}
