using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine.Deprecated;
using Microsoft.Extensions.DependencyInjection;

namespace J4JCommandLine.Tests
{
    public class ServiceProvider
    {
        private static readonly BindingTargetBuilder _btBuilder;

        public static IServiceProvider Instance { get; }

        public static BindingTarget<TValue> GetBindingTarget<TValue>( bool ignoreUnkeyed, TValue? value = null )
            where TValue : class
        {
            _btBuilder.ProgramName( $"{typeof(TValue)}" )
                .IgnoreUnprocessedUnkeyedParameters( ignoreUnkeyed );
            
            return _btBuilder.Build<TValue>( value )!;
        }

        static ServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<SimpleConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            builder.AddTextConverters( typeof(ServiceProvider).Assembly );

            Instance = new AutofacServiceProvider( builder.Build() );

            _btBuilder = Instance.GetRequiredService<BindingTargetBuilder>()
                .Prefixes("-")
                .Quotes('\'', '"')
                .HelpKeys("h")
                .Description("a test program for exercising J4JCommandLine");
        }
    }
}