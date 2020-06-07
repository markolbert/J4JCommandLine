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

            builder.RegisterType<SimpleConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            builder.AddTextConverters( typeof(TestServiceProvider).Assembly );

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}