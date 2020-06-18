# J4JCommandLine
A Net Core 3.1 library for parsing command line arguments 
in a flexible fashion.

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

// create an instance of BindingTarget
var binder = builder.Build<Program>(null);
if (binder == null)
    throw new NullReferenceException( nameof(Program) );

// define your options (here they're bound to public static properties)
// there are fluent calls you can make to add default values, 
// validators, descriptions, etc.
binder.Bind( x => Program.IntValue, "i" );
binder.Bind( x => Program.TextValue, "t" );

// bind the unkeyed parameters -- command line arguments that are
// not options -- if you want to retrieve them (optional but commonly done)
binder.BindUnkeyed( x => Program.Unkeyed );

// parse the command line
if( !binder.Parse( args ) )
{
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine($"IntValue is {IntValue}");
Console.WriteLine($"TextValue is {TextValue}");

Console.WriteLine( Unkeyed.Count == 0
    ? "No unkeyed parameters"
    : $"Unkeyed parameters: {string.Join( ", ", Unkeyed )}" );
```

### Table of Contents

- [Goal and Concept](docs/goal-concept.md)
- [Terminology](docs/terminology.md)
- [Usage](docs/usage.md)
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

