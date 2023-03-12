using System;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using J4JSoftware.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine.Examples
{
    // NOTE: this example is currently identical to StaticPropertyExample. It's
    // here to support future development.
    class Program
    {
        static void Main( string[] args )
        {
            var hostConfig = new J4JHostConfiguration(AppEnvironment.Console)
                             .Publisher( "J4JSoftware" )
                             .ApplicationName( "AutoBindExample" );

            hostConfig.AddCommandLineProcessing( CommandLineOperatingSystems.Windows )
                      .OptionsInitializer( SetupOptions );

            if( hostConfig.MissingRequirements != J4JHostRequirements.AllMet )
            {
                Console.WriteLine( $"Missing J4JHostConfiguration items: {hostConfig.MissingRequirements}" );
                Environment.ExitCode = 1;

                return;
            }

            var host = hostConfig.Build();
            if( host == null )
            {
                Console.WriteLine( "Could not create IJ4JHost" );
                Environment.ExitCode = 1;

                return;
            }

            var config = host.Services.GetRequiredService<IConfiguration>();
            if( config == null )
                throw new NullReferenceException( "Undefined IConfiguration" );

            var help = new ColorHelpDisplay( host.CommandLineLexicalElements!, host.Options! );
            help.Display();

            var parsed = config.Get<Program>();

            if( parsed == null )
            {
                Console.WriteLine( "Parsing failed" );

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine( "Parsing succeeded" );

            Console.WriteLine( $"IntValue is {IntValue}" );
            Console.WriteLine( $"TextValue is {TextValue}" );

            Console.WriteLine( host.Options!.SpuriousValues.Count == 0
                                   ? "No unkeyed parameters"
                                   : $"Unkeyed parameters: {string.Join( ", ", host.Options.SpuriousValues )}" );
        }

        public static int IntValue { get; set; }
        public static string TextValue { get; set; }

        private static void SetupOptions( OptionCollection options )
        {
            options.Bind<Program, int>( x => Program.IntValue, "i" )!
                   .SetDefaultValue( 75 )
                   .SetDescription( "An integer value" );

            options.Bind<Program, string>( x => Program.TextValue, "t" )!
                   .SetDefaultValue( "a cool default" )
                   .SetDescription( "A string value" );
        }
    }
}
