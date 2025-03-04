# What the Library Does...and What It Doesn't Do

When I initially wrote `J4JCommandLine` I envisioned it as a general purpose command line parser which could bind the results of parsing the command line arguments to a variety of targets. Many command line parsers work just that way.

Unfortunately, that kind of general purpose approach is both pretty complicated and has unnecessary complexity if all you're trying to do is incorporate command line parsing into the `ConfigurationBuilder`/`IConfiguration` subsystem.

`J4JCommandLine` does not try to be a general purpose target-whatever-you-want command line processor. Instead, it is designed to incorporate easily into the `ConfigurationBuilder`/`IConfiguration` environment. That introduces a few complexities of its own...but they're buried behind the scenes and you shouldn't have to worry about them.

## Binding Targets

So far as I can tell `IConfiguration` is targeted at creating instances of configuration objects from potentially multiple sources. Since `J4JCommandLine` fits into that environment it can only target objects with public parameterless constructors. It an also only target public properties of those configuration objects. However, the properties can be either static or instance-level in scope.

The targeted property must have both a public getter and a public setter. The `IConfiguration` system apparently allows for non-public setters but I haven't figured out how to handle that situation yet.

Nested properties are allowed and the "intermediate" properties do not have to be publicly writable. They don't even have to be of types with public parameterless constructors, but if they don't have such they must be initialized during construction. Here's an example:

```csharp
public class BasicTargetParameteredCtor
{
    private readonly int _value;

    public BasicTargetParameteredCtor( int value )
    {
        _value = value;
    }

    public bool ASwitch { get; set; }
    public string ASingleValue { get; set; } = string.Empty;
    public List<string> ACollection { get; set; } = new();
    public TestEnum AnEnumValue { get; set; }
    public TestFlagEnum AFlagEnumValue { get; set; }
}

public class EmbeddedTargetNoSetter
{
    public BasicTargetParameteredCtor Target1 { get; } = new( 0 );
    public BasicTargetParameteredCtor Target2 { get; } = new( 0 );
}
```

## Convertability

When I dug through the `IConfiguration` API I was surprised to find there isn't an extensible means of converting text to supported types. Instead, it appears to rely on C\#'s built-in `Convert` object.

`J4JCommandLine` uses the same approach but it does so in a somewhat more organized way so as to allow the addition of custom converters. The required interface for `J4JCommandLine`'s conversion routines is `ITextToValue`. The collection holding all the define conversion routines is defined by `ITextConverters`.

I haven't done much testing of custom converters because it's pretty rare to encounter the need for such. The built-in conversion routines are almost always adequate.

## Incorporating J4JCommandLine into the IConfiguration System

When you look at examples using `J4JCommandLine` you may wonder why the `FinishConfiguration()` call appears after the last option is configured:

```csharp
var optionBuilder = new J4JCommandLineBuilder( StringComparison.OrdinalIgnoreCase, CommandLineOperatingSystem.Windows, null );

optionBuilder.Bind<Program, int>(x => Program.IntValue, "i")!
    .SetDefaultValue(75)
    .SetDescription("An integer value");

optionBuilder.Bind<Program, string>(x => Program.TextValue, "t")!
    .SetDefaultValue("a cool default")
    .SetDescription("A string value");

// adding the option builder to the configuration system triggers parsing of the command line
configBuilder.AddJ4JCommandLineBuilder( optionBuilder );
```
