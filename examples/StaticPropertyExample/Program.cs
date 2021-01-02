using System;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;
#pragma warning disable 8618

namespace J4JSoftware.CommandLine.Examples
{
    public class Program
    {
        static void Main(string[] args)
        {
            var options = new OptionCollection(CommandLineStyle.Linux);

            var intValue = options.Bind<Program, int>(x => Program.IntValue, "i");
            var textValue = options.Bind<Program, string>(x => Program.TextValue, "t");

            var config = new ConfigurationBuilder()
                .AddJ4JCommandLine(args, options)
                .Build();

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
