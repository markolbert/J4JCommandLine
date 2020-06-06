using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;

namespace J4JCommandLine.Tests
{
    public class TestServiceProvider 
    {
        public static IServiceProvider Instance { get; private set; }

        static TestServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SimpleHelpErrorProcessor>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}