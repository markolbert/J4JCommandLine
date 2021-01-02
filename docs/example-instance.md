### Instance Property Example

I generally write console apps that can be configured with some kind of
separate configuration class to hold the configuration information. The 
**J4JCommandLine** framework is designed to bind to a class' public instance
properties.

Here's an example of such a class/app (the source code is in 
examples/InstancePropertyExample). It uses a simple configuration
object:
```
public class Configuration
{
    public int IntValue { get; set; }
    public string TextValue { get; set; }
}
```

The two public properties, **IntValue** and **TextValue**, are bound
to command line options like this:

Here's the app's code:
```csharp
var options = new OptionCollection(CommandLineStyle.Linux);

var intValue = options.Bind<Configuration, int>(x => x.IntValue, "i");
var textValue = options.Bind<Configuration, string>(x => x.TextValue, "t");
```
Often you won't even need to retain the return value from the `Bind<>()` call. It's
generally only useful if you want to set certain properties for the `Option` object:
```csharp
public interface IOption
{
    // ...other details omitted

    Option AddCommandLineKey( string cmdLineKey );
    Option AddCommandLineKeys( IEnumerable<string> cmdLineKeys );
    Option SetStyle( OptionStyle style );
    Option IsRequired();
    Option IsOptional();
    Option SetDescription( string description );

    // ...other details omitted
}
```
Command line keys can be added in the `Bind<>()` call and the binding process 
determines the `OptionStyle` for the option. But you can specifically define an 
option as being required or optional (the default is optional). `SetDescription()`
lets you describe the option, but it's not currently utilized (it's there to 
support future development).

Once you've defined your option collection you can add it to your `IConfiguration`
pipeline by calling `AddJ4JCommandLine()` on an instance of `ConfigurationBuilder`:
```csharp
var config = new ConfigurationBuilder()
    .AddJ4JCommandLine(args, options)
    .Build();
```
At that point you just use the `IConfiguration` framework to get your configuration
objects:
```csharp
var parsed = config.Get<Configuration>();
```
