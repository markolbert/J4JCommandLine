# Changes

## v2.0

Using the library in a number of projects made me realize it was too hard to configure. That was mostly the result of it trying to do too much of the configuration automagically (e.g., scanning a list of provided assemblies to locate alternative interface implementationss and then picking the "best" one). 

I decided the degree of magic had to be reduced significantly...but it wouldn't matter because the programmer would know precisely which implementations he or she wanted to use anyway.

This also allowed me to stop using dependency injection to ensure things got configured correctly. Now you create an `IParser` by doing this:

```csharp
var optionsCollection = new OptionCollection( StringComparison.OrdinalIgnoreCase, new BindabilityValidator() );
var optionsGenerator = new OptionsGenerator( optionsCollection, StringComparison.OrdinalIgnoreCase );
var parsingTable = new ParsingTable( optionsGenerator );
var tokenizer = new Tokenizer( new WindowsLexicalElements() );

var parser = new Parser( optionsCollection, parsingTable, tokenizer );
```

And, if you're willing to rely on the defaults it's even easier:

```csharp
var parser = Parser.GetWindowsDefault();
```

The parser can be added to the `IConfiguration` system so that you can use its `IConfiguration.Get<T>()` syntax:

```csharp
var configRoot = new ConfigurationBuilder()
    .AddJ4JCommandLine( parser, out var options )
    .Build();

// binds /x to ASwitch
options.Bind<Target, bool>(x => x.ASwitch, "x");

// bind /t to ASingleValue (which is a string)
options.Bind<Target, string>(x => x.ASingleValue, "t");

options.FinishConfiguration();

var target1 = configRoot.Get<Target>();
```

## v1.1

You can now display help on the console:

```csharp
var options = new OptionCollection(CommandLineStyle.Linux);

var intValue = options.Bind<Program, int>( x => Program.IntValue, "i" )!
    .SetDefaultValue( 75 )
    .SetDescription( "An integer value" );

var textValue = options.Bind<Program, string>( x => Program.TextValue, "t" )!
    .SetDefaultValue( "a cool default" )
    .SetDescription( "A string value" );

options.DisplayHelp();

options.DisplayHelp( new DisplayColorHelp() );
```

Calling `DisplayHelp()` without a display formatter gives you a very simple display:
![simple help](/assets/simple-help.png)

Calling `DisplayHelp()` with the `DisplayColorHelp()` formatter gives you this:
![colorful help](/assets/fancy-help.png)

`DisplayColorHelp` is defined in an add-on assembly, *ColorfulHelp*. It's based on [CsConsoleFormat](https://github.com/Athari/CsConsoleFormat), a cool library that makes it easy to colorize and structure console
output.

Default values can now be specified and will be used if nothing is entered at the command line for an option.

I extracted the property validation logic (i.e., the code which determines whether a property can be bound to an option). This doesn't have any user-side impact. It's part of a longer-term effort to proposing a modification to the overall Net5 `IConfiguation` system I'm working on.

## v1.0

The original version of this library did not integrate easily with the Net5
IConfiguration system. In addition, its error-checking/validation capabilities
required a lot of complex, behind-the-scenes code. That was, in part, due to its
duplicating some of the functionality of the IConfiguration system, as well as
doing validations which arguably ought to be done in application code.

Going forward I don't plan on any further development for the original version. It's
available as version 0.5.0.1.
