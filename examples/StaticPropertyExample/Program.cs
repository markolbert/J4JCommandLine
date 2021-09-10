using System;
using System.Linq;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using Microsoft.Extensions.Configuration;
#pragma warning disable 8618

namespace J4JSoftware.CommandLine.Examples
{
    public class Program
    {
        static void Main(string[] args)
        {
            var options = CompositionRoot.Default.CommandLineOptions;
            if (options == null)
                throw new NullReferenceException("Undefined IOptionCollection");

            var config = CompositionRoot.Default.Configuration;
            if (config == null)
                throw new NullReferenceException("Undefined IConfiguration");

            var help = new HelpDisplayColor( options );
            help.Display();

            var parsed = config.Get<Program>();

            if (parsed == null)
            {
                Console.WriteLine("Parsing failed");

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine("Parsing succeeded");

            Console.WriteLine($"IntValue is {IntValue}");
            Console.WriteLine($"TextValue is {TextValue}");

            Console.WriteLine(options.UnkeyedValues.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join(", ", options.UnkeyedValues)}");
        }

        public static int IntValue { get; set; }
        public static string TextValue { get; set; }
    }
}
