# J4JCommandLine

A Net library which adds parsing command line arguments to the `IConfiguration` system. Command line arguments can be bound to instance or static properties of classes, including nested classes.

There are some restrictions on the nature of the target classes, mostly having to do with them having public parameterless constructors. This is the same constraint as exists for the `IConfiguration` system.

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.Configuration.CommandLine?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.Configuration.CommandLine/)

The libraries are licensed under the GNU GPL-v3 or later. For more details see the [license file](../../LICENSE.md).

See the [change log](changes.md) for a history of significant changes.

## TL;DR

```csharp
// a static field is used so I can sequester option configuration
// into a method call. you could simply make it an inline variable
// within Main()
private static J4JCommandLineBuilder? _optionBuilder;

// needed so we can display help. 
private static ILexicalElements? _tokens;

static void Main( string[] args )
{
    // you don't have to use Autofac as your DI system, but I love it!
    // SetupConfiguration() defines the options. It's called twice because
    // of the way the IHostBuilder system works (the first time is for initial
    // setup, the second time is for final configuration)
    var builder = new HostBuilder()
                    .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureHostConfiguration(SetupConfiguration)
                    .ConfigureAppConfiguration(SetupConfiguration);

    var host = builder.Build();

    if (_optionBuilder == null || _tokens == null)
    {
        Console.WriteLine("Option setup failed");

        Environment.ExitCode = -1;
        return;
    }

    var config = host.Services.GetRequiredService<IConfiguration>();
    if( config == null )
        throw new NullReferenceException( "Undefined IConfiguration" );

    var help = new ColorHelpDisplay(_tokens, _optionBuilder.Options);
    help.Display();

    // grabs an instance of the configured target, which in this case
    // is ourself
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
    Console.WriteLine( $"SwitchValue is {SwitchValue}" );

    // this confirms that everything that should've been parsed
    // was actually parsed to the target properties
    Console.WriteLine(_optionBuilder.Options.SpuriousValues.Count == 0
                            ? "No unkeyed parameters"
                            : $"Unkeyed parameters: {string.Join(", ", _optionBuilder.Options.SpuriousValues)}");
}

public static int IntValue { get; set; }
public static string TextValue { get; set; }
public static bool SwitchValue { get; set; }

// the DI call which configures option parsin
private static void SetupConfiguration( IConfigurationBuilder configBuilder )
{
    // it all starts by creating an instance of an option builder class
    // you can specifically identify the OS you're working in via a 
    // second parameter to the constructor, but it defaults to assuming
    // you're using Windows
    _optionBuilder = new J4JCommandLineBuilder( StringComparison.OrdinalIgnoreCase );

    _optionBuilder.Bind<Program, int>( x => Program.IntValue, "i" )!
            .SetDefaultValue( 75 )
            .SetDescription( "An integer value" );

    _optionBuilder.Bind<Program, string>( x => Program.TextValue, "t" )!
        .SetDefaultValue( "a cool default" )
        .SetDescription( "A string value" );

    _optionBuilder.Bind<Program, bool>(x => Program.SwitchValue, "s")!
        .SetDefaultValue(false)
        .SetDescription("A switch");

    // this next call is what triggers the parsing of the command line

    // passing in an instance of ILexicalElements by reference is only
    // needed if you're going to display a help screen...which, of course, 
    // you're generally going to want to do in a console app like this.

    // there's a variant of AddJ4JCommandLine() that doesn't include the 
    // ref tokens argument, which can be used if you don't need to display
    // help screens
    ILexicalElements? tokens = null;
    configBuilder.AddJ4JCommandLine(_optionBuilder, ref tokens);
    _tokens = tokens;
}
```

There is also a `TryBind<...>()` method which you can use as an alternative to `Bind<...>()`.

## Table of Contents

- [Changes](changes.md)
- [What It Does and Doesn't Do](doesdonts.md)
- [Goal and Concept](goal-concept.md)
- [Command Line Styles](cmdlinestyle.md)
- [Binding](binding.md)
- [Logging and Errors](logging.md)
- [Outputting Help](help.md)
- [Debugging and Testing](debugging.md)
- [Notes on the Tokenizer and Parser](parser.md)
