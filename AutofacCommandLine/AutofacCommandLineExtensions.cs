using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;

namespace J4JSoftware.CommandLine
{
    public static class AutofacCommandLineExtensions
    {
        public static ContainerBuilder AddJ4JCommandLine( this ContainerBuilder builder )
        {
            var outputConfig = new OutputConfiguration()
            {
                DetailAreaWidth = 58,
                KeyAreaWidth = 20,
                KeyDetailSeparation = 2
            };

            builder.AddJ4JCommandLine( new ParsingConfiguration() )
                .AddDefaultParser()
                .AddSimpleHelpErrorProcessor( outputConfig )
                .AddTextConverters( typeof(ITextConverter).Assembly );

            return builder;
        }

        public static ContainerBuilder AddJ4JCommandLine<TConfig>(
            this ContainerBuilder builder,
            TConfig parsingConfig )
            where TConfig : class, IParsingConfiguration
        {
            builder.Register( c => parsingConfig )
                .As<IParsingConfiguration>()
                .SingleInstance();

            builder.RegisterGeneric( typeof(BindingTarget<>) )
                .As( typeof(IBindingTarget<>) );

            return builder;
        }

        public static ContainerBuilder AddDefaultParser( this ContainerBuilder builder )
        {
            builder.RegisterType<CommandLineParser>()
                .As<ICommandLineTextParser>()
                .SingleInstance();

            builder.RegisterType<ElementTerminator>()
                .As<IElementTerminator>()
                .SingleInstance();

            builder.RegisterType<KeyPrefixer>()
                .As<IElementKey>()
                .SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddHelpErrorProcessor<TConfig, THelp>(
            this ContainerBuilder builder,
            TConfig outputConfig)
            where TConfig : class
            where THelp : class, IHelpErrorProcessor
        {
            builder.Register( c => outputConfig )
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<THelp>()
                .AsImplementedInterfaces()
                .SingleInstance();

            return builder;
        }

        public static ContainerBuilder AddSimpleHelpErrorProcessor(
            this ContainerBuilder builder,
            OutputConfiguration outputConfig )
        {
            builder.Register( c => outputConfig )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<SimpleHelpErrorProcessor>()
                .As<IHelpErrorProcessor>()
                .SingleInstance();

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
