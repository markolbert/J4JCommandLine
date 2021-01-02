### Static Property Example

The most likely place you'd use a command line parser is in a console
app. C# console apps are typically defined via static methods (e.g., in a
**Program** class) so you want to bind to a class' public static properties.

The process is very similar to what you do for an 
[instance-based](example-instance.md) approach but it's a little counter-intuitive.
Here's an example:
```csharp
var options = new OptionCollection(CommandLineStyle.Linux);

var intValue = options.Bind<Program, int>(x => Program.IntValue, "i");
var textValue = options.Bind<Program, string>(x => Program.TextValue, "t");

var config = new ConfigurationBuilder()
    .AddJ4JCommandLine(args, options)
    .Build();

var parsed = config.Get<Program>();
```
The approach is almost identical to the instance-based one, except that the
binding expression ignores the instance variable (**x** in the example). Instead,
you reference the static properties of the main program class (**Program** in the
example).
```
