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
    .HelpKeys( "h", "?" );

// create an instance of BindingTarget
builder.Build<Program>(null, out var binder, out var _);

if( binder == null )
    throw new NullReferenceException( nameof(Program) );

// define your options (here they're bound to public static properties)
// there are fluent calls you can make to add default values, 
// validators, descriptions, etc.
binder.Bind( x => Program.IntValue, "i" );
binder.Bind( x => Program.TextValue, "t" );

// parse the command line
if( binder.Parse( args ) != MappingResults.Success )
{
    Environment.ExitCode = 1;
    return;
}

Console.WriteLine($"IntValue is {IntValue}");
Console.WriteLine($"TextValue is {TextValue}");
```

### Table of Contents

- [Goal and Concept](docs/goal-concept.md)
- [Usage](docs/usage.md)
- Examples
  - [Binding to static properties]
  - [Binding to a configuration object]
  - [Indirectly binding to a method]
- [Structural Diagram]
- Extending the Framework
  - [Text Converters]
- Notes
  - [Command line parser]

#### Inspiration and Dedication

This library is dedicated to Jon Sequitur, one of the leads on
Microsoft's own command line parser, for his patient explanations
of how that other parser is designed and his suggestions for
potential changes to it which eventually led to me writing
J4JCommandLine.

