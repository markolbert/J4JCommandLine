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

            builder.RegisterType<ParsingConfiguration>()
                .OnActivated(ae =>
                {
                    ae.Instance.Description = "a description of the program";
                    ae.Instance.ProgramName = "program.exe";
                })
                .As<IParsingConfiguration>()
                .SingleInstance();

            builder.Register( c => new OutputConfiguration()
                {
                    DetailAreaWidth = 58,
                    KeyAreaWidth = 20,
                    KeyDetailSeparation = 2
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterGeneric( typeof( BindingTarget<> ) )
                .As( typeof( IBindingTarget<> ) );

            builder.RegisterType<SimpleHelpErrorProcessor>()
                .As<IHelpErrorProcessor>()
                .SingleInstance();

            builder.RegisterType<CommandLineParser>()
                .As<ICommandLineTextParser>()
                .SingleInstance();

            builder.RegisterType<ElementTerminator>()
                .As<IElementTerminator>()
                .SingleInstance();

            builder.RegisterType<KeyPrefixer>()
                .As<IElementKey>()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(ITextConverter).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ITextConverter).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
                .AsImplementedInterfaces();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}