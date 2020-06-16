using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppJ4JCmdLine
{
    public class Program
    {
        static void Main(string[] args)
        {
            InitializeServiceProvider();

            var builder = ServiceProvider.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes( "-", "--", "/" )
                .Quotes( '\'', '"' )
                .HelpKeys( "h", "?" )
                .Description( "a test program for exercising J4JCommandLine" )
                .ProgramName( $"{nameof(Program)}.exe" );

            var binder = builder.Build<Program>(null);
            if (binder == null)
                throw new NullReferenceException( nameof(Program) );

            binder.Bind( x => Program.IntValue, "i" )
                .SetDescription( "an integer value" )
                .SetDefaultValue( 1 )
                .SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

            binder.Bind( x => Program.TextValue, "t" )
                .SetDescription( "a text value" )
                .SetDefaultValue( "some text value" );

            if( binder.Parse( args ) != MappingResults.Success )
            {
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine($"IntValue is {IntValue}");
            Console.WriteLine($"TextValue is {TextValue}");
        }

        public static IServiceProvider ServiceProvider { get; set; }
        public static int IntValue { get; set; }
        public static string TextValue { get; set; }

        private static void InitializeServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FancyConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            ServiceProvider = new AutofacServiceProvider(builder.Build());
        }
    }
}
