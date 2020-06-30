using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.CommandLine.Examples
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

            binder.BindUnkeyed( x => Program.Unkeyed );

            if( !binder.Parse( args ) )
            {
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine($"IntValue is {IntValue}");
            Console.WriteLine($"TextValue is {TextValue}");

            Console.WriteLine( Unkeyed.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join( ", ", Unkeyed )}" );
        }

        public static IServiceProvider ServiceProvider { get; set; }
        public static int IntValue { get; set; }
        public static string TextValue { get; set; }
        public static List<string> Unkeyed { get; set; }

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
