using System;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using Microsoft.Extensions.Configuration;

namespace NoDIExample
{
    class Program
    {
        static void Main( string[] args )
        {
            var config = new ConfigurationBuilder()
                         .AddJ4JCommandLineForWindows( out var options, out _ )
                         .Build();

            options!.Bind<Program, int>( x => Program.IntValue, "i" )!
                    .SetDefaultValue( 75 )
                    .SetDescription( "An integer value" );

            options.Bind<Program, string>( x => Program.TextValue, "t" )!
                   .SetDefaultValue( "a cool default" )
                   .SetDescription( "A string value" );

            options.FinishConfiguration();

            var help = new ColorHelpDisplay( new WindowsLexicalElements(), options );
            help.Display();

            var parsed = config.Get<Program>();

            if ( parsed == null )
            {
                Console.WriteLine( "Parsing failed" );

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine( "Parsing succeeded" );

            Console.WriteLine( $"IntValue is {IntValue}" );
            Console.WriteLine( $"TextValue is {TextValue}" );

            Console.WriteLine( options.SpuriousValues.Count == 0
                                   ? "No unkeyed parameters"
                                   : $"Unkeyed parameters: {string.Join( ", ", options.SpuriousValues )}" );
        }

        public static int IntValue { get; set; }
        public static string TextValue { get; set; } = string.Empty;
    }
}
