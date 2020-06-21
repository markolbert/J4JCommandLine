# J4JCommandLine
A Net Core 3.1 library for parsing command line arguments 
in a flexible fashion.

![Nuget](https://img.shields.io/nuget/v/J4JSoftware.CommandLine?style=flat-square)

### TL;DR

```
// create an instance of BindingTargetBuilder (here done via 
// dependency injection). Note that there are several objects
// you need to create if you want to create an instance of
// BindingTargetBuilder without dependency injection.
var builder = ServiceProvider.GetRequiredService<BindingTargetBuilder>();

// configure the builder (these are the minimum calls you need to make)
builder.Prefixes( "-", "--", "/" )
    .Quotes( '\'', '"' )
    .HelpKeys( "h", "?" );

var binder = builder.AutoBind<Program>();
if (binder == null)
    throw new NullReferenceException(nameof(Program));

binder.Options[ "i" ]!.SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

if (!binder.Parse(args))
{
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine($"IntValue is {IntValue}");
Console.WriteLine($"TextValue is {TextValue}");

Console.WriteLine(Unkeyed.Count == 0
    ? "No unkeyed parameters"
    : $"Unkeyed parameters: {string.Join(", ", Unkeyed)}");
```

### Table of Contents

- [Goal and Concept](docs/goal-concept.md)
- [Terminology](docs/terminology.md)
- Usage
  - [Autobinding](docs/usage-auto.md)
  - [Explicit binding](docs/usage-explicit.md)
- [How Command Lines Are Allocated](docs/allocation.md)
- Examples
  - [Binding to static properties](docs/example-static.md)
  - [Binding to a configuration object](docs/example-instance.md)
- [Architectural Notes](docs/diagrams.md)
- [Autofac Dependency Injection Support](docs/di.md)
- Extending the framework
  - [Adding Text Converters](docs/text-converters.md)
  - [Adding Validators](docs/validators.md)
- [Notes on the allocator](docs/allocator.md)

#### Inspiration and Dedication

This library is dedicated to Jon Sequitur, one of the leads on
Microsoft's own command line parser, for his patient explanations
of how that other parser is designed and his suggestions for
potential changes to it which eventually led to me writing
J4JCommandLine.

