### Binding

At a basic level all the `J4JCommandLine` library does is map `IConfiguration`
targets to command line options, and then parse a command line to those mappings.
The mapping part is what the `Bind<>()` method calls do, by walking the property
expressions back up to the root class that defines them. Example:
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

For this to work the target properties must be publicly read/writable and the
"intervening" classes (e.g., `SubClass` in the example) must have public
parameterless constructors. `IConfiguration` allows for non-public accessors but
I'm still working on that in `J4JCommandLine`. The public parameterless constructor
constraint is an `IConfiguration` requirement.

You can set up the bindings manually as well:
```csharp
var option1 = optionCollection.Add("AProperty")
    .AddCommandLineKey( "a" );
var option2 = optionCollection.Add("SubClass:AnotherProperty")
    .AddCommandLineKey( "b" );
```
but it's not as convenient. Or intuitive, IMHO.