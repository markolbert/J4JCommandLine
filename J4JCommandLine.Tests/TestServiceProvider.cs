using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoFacJ4JLogging;
using J4JSoftware.CommandLine;
using J4JSoftware.Logging;

namespace J4JCommandLine.Tests
{
    public class TestServiceProvider 
    {
        public static IServiceProvider Instance { get; private set; }

        static TestServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<OptionCollection>()
                .As<IOptionCollection>()
                .SingleInstance();

            builder.RegisterType<ParsingConfiguration>()
                .As<IParsingConfiguration>()
                .SingleInstance();

            builder.Register( c => new OutputConfiguration( null )
                {
                    DetailAreaWidth = 55,
                    KeyAreaWidth = 20,
                    KeyDetailSeparation = 5
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<SimpleConsoleHelpErrorProcessor>()
                .As<IHelpErrorProcessor>()
                .SingleInstance();

            builder.RegisterType<CommandLineContext>()
                .AsSelf();

            builder.RegisterType<CommandLineTextParser>()
                .As<ICommandLineTextParser>()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(ITextConverter).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ITextConverter).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
                .AsImplementedInterfaces();

            builder.Register(c =>
               {
                   var retVal = new J4JLoggerConfiguration
                   {
                       EventElements = EventElements.All,
                   };

                   retVal.Channels.Add(new ConsoleChannel());

                   return retVal;
               })
                .As<IJ4JLoggerConfiguration>()
                .SingleInstance();

            builder.AddJ4JLogging();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}