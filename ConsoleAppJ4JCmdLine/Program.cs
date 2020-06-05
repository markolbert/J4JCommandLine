using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
            var builder = new ContainerBuilder();

            var outputConfig = new FancyOutputConfiguration();

            builder.AddJ4JCommandLine(new ParsingConfiguration())
                .AddDefaultParser()
                .AddHelpErrorProcessor<FancyOutputConfiguration, FancyHelpErrorProcessor>(outputConfig)
                .AddTextConverters(typeof(ITextConverter).Assembly);

            ServiceProvider = new AutofacServiceProvider( builder.Build() );

            var binder = ServiceProvider.GetRequiredService<IBindingTarget<Program>>();

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
    }
}
