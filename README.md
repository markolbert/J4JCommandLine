# J4JCommandLine
A Net5 library which adds parsing command line arguments to the IConfiguration
system.

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.Configuration.CommandLine?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.Configuration.CommandLine/)

### TL;DR

```csharp
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
```
### v1.0 Breaking Changes
The original version of this library did not integrate easily with the Net5
IConfiguration system. In addition, its error-checking/validation capabilities
required a lot of complex, behind-the-scenes code. That was, in part, due to its
duplicating some of the functionality of the IConfiguration system, as well as
doing validations which arguably ought to be done in application code.

Going forward I don't plan on any further development for the original version. It's
available as version 0.5.0.1.

### Table of Contents

- [Goal and Concept](docs/goal-concept.md)
- [Command Line Styles](docs/cmdlinestyle.md)
- [Binding](docs/binding.md)
- Examples
  - [Binding to static properties](docs/example-static.md)
  - [Binding to a configuration object](docs/example-instance.md)
- [Logging and Errors](docs/logging.md)
- [Notes on the Tokenizer and Parser](docs/parser.md)

#### Inspiration and Dedication

This library is dedicated to Jon Sequitur, one of the leads on
Microsoft's own command line parser, for his patient explanations
of how that other parser is designed and his suggestions for
potential changes to it which eventually led to me writing
J4JCommandLine.

