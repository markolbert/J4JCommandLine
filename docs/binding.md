# Binding

## Overview

At a basic level all the `J4JCommandLine` library does is map `IConfiguration` targets to command line options, and then parse a command line to those mappings. The mapping part is what the `Bind<>()` method calls do, by walking the property expressions back up to the root class that defines them. Example:

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

var option1 = optionCollection.Bind<RootClass, int>( x => x.AProperty, "a" );
var option2 = optionCollection.Bind<RootClass, int>( x => x.SubClass.AnotherProperty, "b" );
```

The binding calls create `IConfiguration` mappings like this:

- option1 => "AProperty"
- option2 => "SubClass:AnotherProperty"

For this to work the target properties must be publicly read/writable and the "intervening" classes (e.g., `SubClass` in the example) must have public parameterless constructors. `IConfiguration` allows for non-public accessors but I'm still working on that in `J4JCommandLine`. The public parameterless constructor constraint is an `IConfiguration` requirement.

## Option Constraints

You specify optional constraints on an option via the following extension-style methods.

```csharp
IOption AddCommandLineKey( string cmdLineKey );
IOption AddCommandLineKeys( IEnumerable<string> cmdLineKeys );
```

Adds one or more keys by which the option can be set on the command line (a key is the 'x' part of '/x').

Keys must be unique across the option collection. Case sensitivity depends on the settings used to create the option collection. The Windows default is case-insensitive. The Linux default is case sensitive.

Specifying a key already in use will result in it being ignored. Keys do not need to be single characters.

```csharp
IOption IsRequired();
IOption IsOptional();
```

These extension methods make an option either required or optional. The default is optional.

```csharp
IOption SetDescription( string description );
```

Sets a description for the option which can be displayed by the help subsystem.
