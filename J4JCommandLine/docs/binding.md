# Binding

## Overview

At a basic level all the `J4JCommandLine` library does is map `IConfiguration` targets to command line options, and then parse a command line to those mappings. The mapping part is set up by the `Bind<>()` method calls, by walking the property expressions back up to the root class that defines them. Example:

```csharp
public class RootClass
{
    public int AProperty { get; set; }
    public SubClass SubClass { get; set; }
}

public class SubClass
{
    public int AnotherProperty { get; set; }
}

// this next line creates the option builder used to define options
var optionBuilder = new J4JCommandLineBuilder( StringComparison.OrdinalIgnoreCase, CommandLineOperatingSystem.Windows, null) ;

var option1 = optionBuilder.Bind<RootClass, int>( x => x.AProperty, "a" );
var option2 = optionBuilder.Bind<RootClass, int>( x => x.SubClass.AnotherProperty, "b" );

// adding the option builder to the IConfiguration builder is what triggers parsing the command line
configBuilder.AddJ4JCommandLine( optionBuilder, )
```

The binding calls create `IConfiguration` mappings like this:

- option1 => "AProperty"
- option2 => "SubClass:AnotherProperty"

For this to work the target properties must be publicly read/writable and the "intervening" classes (e.g., `SubClass` in the example) must have public parameterless constructors. `IConfiguration` allows for non-public accessors but I'm still working on that in `J4JCommandLine`. The public parameterless constructor constraint is an `IConfiguration` requirement.

## Option Constraints

You specify optional constraints on an option via the following extension-style methods.

### Adding Option Keys

```csharp
public Option<TContainer, TProp> AddCom0mandLineKey( string cmdLineKey ){...}
public Option<TContainer, TProp> AddCommandLineKeys( IEnumerable<string> cmdLineKeys ) {...}
```

Adds one or more keys by which the option can be set on the command line (a key is the 'x' part of '/x').

Keys must be unique across the option collection. Case sensitivity depends on the settings used to create the option collection. The Windows default is case-insensitive. The Linux default is case sensitive.

Specifying a key already in use will result in it being ignored. Keys do not need to be single characters.

### Making an Option Required or Optional

```csharp
IOption IsRequired();
IOption IsOptional();
```

These extension methods make an option either required or optional. The default is optional.

### Providing a Description

```csharp
IOption SetDescription( string description );
```

Sets a description for the option which can be displayed by the help subsystem.
