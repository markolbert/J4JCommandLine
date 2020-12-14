using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Binder.Tests;
using J4JSoftware.CommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Twilio.Rest.Api.V2010.Account.Usage.Record;
using Xunit.Sdk;

namespace Binder.Tests
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

            builder.RegisterType<Options>()
                .AsSelf();

            builder.RegisterType<MasterTextCollection>()
                .OnActivated( x =>
                {
                    x.Instance.Initialize( StringComparison.OrdinalIgnoreCase );
                    x.Instance.AddRange( TextUsageType.Prefix, "-", "--" );
                    x.Instance.AddRange( TextUsageType.Quote, "\"", "'" );
                    x.Instance.AddRange( TextUsageType.ValueEncloser, "=" );
                } )
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<DefaultTypeInitializer>()
                .AsImplementedInterfaces();

            builder.RegisterType<Allocator>()
                .AsImplementedInterfaces();

            builder.RegisterType<ElementTerminator>()
                .AsImplementedInterfaces();

            builder.RegisterType<KeyPrefixer>()
                .AsImplementedInterfaces();
        }

        public IJ4JLogger Logger => Host!.Services.GetRequiredService<IJ4JLogger>();
        public Options Options => Host!.Services.GetRequiredService<Options>();
        public IAllocator Allocator => Host!.Services.GetRequiredService<IAllocator>();
    }
}
