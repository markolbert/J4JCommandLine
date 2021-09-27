# J4JCommandLine

A Net5 library which adds parsing command line arguments to the `IConfiguration` system. Command line arguments can be bound to instance or static properties of classes, including nested classes. 

There are some restrictions on the nature of the target classes, mostly having to do with them having public parameterless constructors. This is the same constraint as exists for the `IConfiguration` system.

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.Configuration.CommandLine?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.Configuration.CommandLine/)

The libraries are licensed under the GNU GPL-v3 or later. For more details see the [license file](LICENSE.md).

**There are breaking changes in the latest release. Please review the documentation.**

## TL;DR

```csharp
static void Main(string[] args)
{
    var config = new ConfigurationBuilder()
        .AddJ4JCommandLineForWindows( out var options, out _ )
        .Build();

    options!.Bind<Program, int>(x => Program.IntValue, "i")!
        .SetDefaultValue(75)
        .SetDescription("An integer value");

    options.Bind<Program, string>(x => Program.TextValue, "t")!
        .SetDefaultValue("a cool default")
        .SetDescription("A string value");

    options.FinishConfiguration();

    var help = new ColorHelpDisplay(new WindowsLexicalElements(), options);
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

    Console.WriteLine(options.SpuriousValues.Count == 0
        ? "No unkeyed parameters"
        : $"Unkeyed parameters: {string.Join(", ", options.SpuriousValues)}");
}

public static int IntValue { get; set; }
public static string TextValue { get; set; }
```

This example can be simplified further by using my [dependency injection library](https://github.com/markolbert/ProgrammingUtilities). There is also a `TryBind<...>()` method which you can use as an alternative to `Bind<...>()`.

## Table of Contents

- [Changes](docs/changes.md)
- [What It Does and Doesn't Do](docs/doesdonts.md)
- [Goal and Concept](docs/goal-concept.md)
- [Command Line Styles](docs/cmdlinestyle.md)
- [Binding](docs/binding.md)
- [Logging and Errors](docs/logging.md)
- [Outputting Help](docs/help.md)
- [Notes on the Tokenizer and Parser](docs/parser.md)

## Inspiration and Dedication

This library is dedicated to Jon Sequitur, one of the leads on Microsoft's own command line parser, for his patient explanations of how that other parser is designed and his suggestions for potential changes to it which eventually led to me writing `J4JCommandLine`.
