﻿using System;
using J4JSoftware.Configuration.CommandLine;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new OptionCollection(CommandLineStyle.Linux);

            var intValue = options.Bind<Configuration, int>(x => x.IntValue, "i");
            var textValue = options.Bind<Configuration, string>(x => x.TextValue, "t");

            var config = new ConfigurationBuilder()
                .AddJ4JCommandLine(args, options)
                .Build();

            var parsed = config.Get<Configuration>();

            if (parsed == null)
            {
                Console.WriteLine("Parsing failed");

                Environment.ExitCode = -1;
                return;
            }

            Console.WriteLine("Parsing succeeded");

            Console.WriteLine($"IntValue is {parsed.IntValue}");
            Console.WriteLine($"TextValue is {parsed.TextValue}");

            Console.WriteLine(options.UnkeyedValues.Count == 0
                ? "No unkeyed parameters"
                : $"Unkeyed parameters: {string.Join(", ", options.UnkeyedValues)}");
        }
    }
}
