using System;
using System.Linq;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using Microsoft.Extensions.Configuration;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine.Examples
{
    // NOTE: this example is currently identical to StaticPropertyExample. It's
    // here to support future development.
    class Program
    {
        static void Main( string[] args )
        {
            var parser = CompositionRoot.Default.Parser;
            var displayHelp = CompositionRoot.Default.DisplayHelp;

            var intValue = parser.Options.Bind<Program, int>( x => Program.IntValue, "i" )!
                .SetDefaultValue( 75 )
                .SetDescription( "An integer value" );

            var textValue = parser.Options.Bind<Program, string>( x => Program.TextValue, "t" )!
                .SetDefaultValue( "a cool default" )
                .SetDescription( "A string value" );

            parser.Options.DisplayHelp( displayHelp );
            Console.WriteLine("\n===============\n");
            parser.Options.DisplayHelp( new DisplayColorHelp() );

            var config = new ConfigurationBuilder()
                .AddJ4JCommandLine(CommandLineStyle.Windows, CompositionRoot.Default.Host!.Services, out _ )
                .Build();

            var parsed = config.Get<Program>();

            if( parsed == null )
            {
                Console.WriteLine( "Parsing failed" );

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine( "Parsing succeeded" );

            Console.WriteLine( $"IntValue is {IntValue}");
            Console.WriteLine( $"TextValue is {TextValue}");

            var provider = config.Providers
                .FirstOrDefault( x => x is J4JCommandLineProvider ) as J4JCommandLineProvider;

            var options = provider!.Source.Parser!.Options;
    
            Console.WriteLine( options.UnkeyedValues.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join( ", ", options.UnkeyedValues )}" );
        }

        public static int IntValue { get; set; }
        public static string TextValue { get; set; }
    }
}
