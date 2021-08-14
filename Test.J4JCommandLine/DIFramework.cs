using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Frameworks.Autofac;

[assembly: TestFramework("J4JSoftware.Binder.Tests.DIFramework", "J4JSoftware.J4JCommandLine.Binder.Tests")]
namespace J4JSoftware.Binder.Tests
{
    public class DIFramework : AutofacTestFramework
    {
        public DIFramework( IMessageSink diagnosticMessageSink ) 
            : base( diagnosticMessageSink )
        {
        }

        protected override void ConfigureContainer( ContainerBuilder builder )
        {
            // register J4JLogger, as both itself and as IJ4JLogger,
            // because its configuration depends on being able to resolve
            // more than just the interface
            builder.RegisterType<J4JLogger>()
                .OnActivated(x => ConfigureLogger(x.Instance))
                .As<IJ4JLogger>()
                .AsSelf()
                .SingleInstance();

            // Register logging channels. This also updates the protected property RegisteredLoggerChannelTypes
            // so resolution of channel types doesn't require access to the builder context
            builder.RegisterAssemblyTypes(typeof(IJ4JLogger).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IChannel).IsAssignableFrom(t)
                            && t.GetConstructors().Any(c => c.GetParameters().Length == 0 ))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.RegisterModule(new AutofacModule());
            builder.RegisterTextToValueAssemblies();
            builder.RegisterTokenAssemblies();
            builder.RegisterMasterTextCollectionAssemblies();
            builder.RegisterBindabilityValidatorAssemblies();
            builder.RegisterDisplayHelpAssemblies();
        }

        private void ConfigureLogger(J4JLogger logger)
        {
            logger.AddDebug();
        }

    }
}
