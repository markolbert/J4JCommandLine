using System;
using System.Reflection;
using Autofac;

namespace J4JSoftware.CommandLine
{
    public static class AutofacCommandLineExtensions
    {
        public static ContainerBuilder AddJ4JCommandLine( this ContainerBuilder builder )
        {
            builder.RegisterType<KeyPrefixer>()
                .AsImplementedInterfaces();

            builder.RegisterType<ElementTerminator>()
                .AsImplementedInterfaces();

            builder.RegisterType<Allocator>()
                .AsImplementedInterfaces();

            builder.RegisterType<BindingTargetBuilder>()
                .AsSelf();

            builder.AddTextConverters( typeof(ITextConverter).Assembly );

            return builder;
        }

        public static ContainerBuilder AddTextConverters( this ContainerBuilder builder, Assembly toScan )
        {
            builder.RegisterAssemblyTypes(toScan)
                .Where(t => !t.IsAbstract
                            && typeof(ITextConverter).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            return builder;
        }

        public static ContainerBuilder AddTextConverters(this ContainerBuilder builder, params Type[] textConverters )
        {
            foreach( var textConv in textConverters )
            {
                if( textConv.IsAbstract
                    || !typeof(ITextConverter).IsAssignableFrom( textConv )
                    || textConv.GetConstructors().Length == 0 )
                    continue;

                builder.RegisterType( textConv )
                    .AsImplementedInterfaces();
            }

            return builder;
        }
    }
}
