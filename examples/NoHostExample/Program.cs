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
            var optionBuilder = new J4JCommandLineBuilder(StringComparison.OrdinalIgnoreCase);

            optionBuilder.Bind<Program, int>(x => Program.IntValue, "i")!
                         .SetDefaultValue(75)
                         .SetDescription("An integer value");

            optionBuilder.Bind<Program, string>(x => Program.TextValue, "t")!
                         .SetDefaultValue("a cool default")
                         .SetDescription("A string value");

            ILexicalElements? tokens = null;
            var config = new ConfigurationBuilder()
                        .AddJ4JCommandLine( optionBuilder, ref tokens )
                        .Build();

            if (tokens == null)
            {
                Console.WriteLine("Option setup failed");

                Environment.ExitCode = -1;
                return;
            }

            var help = new ColorHelpDisplay( tokens, optionBuilder.Options );
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

            Console.WriteLine( optionBuilder.Options.SpuriousValues.Count == 0
                                   ? "No unkeyed parameters"
                                   : $"Unkeyed parameters: {string.Join( ", ", optionBuilder.Options.SpuriousValues )}" );
        }

        public static int IntValue { get; set; }
        public static string TextValue { get; set; } = string.Empty;
    }
}
