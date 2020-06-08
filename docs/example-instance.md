### Instance Property Example

I generally write console apps that can be configured with some kind of
separate configuration class to hold the configuration information. The 
**J4JCommandLine** framework is designed to bind to a class' public instance
properties.

Here's an example of such a class/app (the source code is in 
examples/StaticPropertyExample). It's using [Autofac](https://autofac.org) as
the dependency injection system and the extension library *AutofacCommandLine*
that's available in the repository. It uses a simple configuration
object:
```
public class Configuration
{
    public int IntValue { get; set; }
    public string TextValue { get; set; }
}
```

The two public properties, **IntValue** and **TextValue** are bound
to command line options in code I'll show below. Otherwise there's nothing
special about them. They're just regular properties with public get
and set accessors.

Here's the app's code:
```
using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace InstancePropertyExample
{
    class Program
    {
        static IServiceProvider _svcProvider { get; set; }

        static void Main(string[] args)
        {
            // detail follows below, omitted here for brevity...
        }

        private static void InitializeServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FancyConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            _svcProvider = new AutofacServiceProvider(builder.Build());
        }
    }
}
```

The **InitializeServiceProvider()** method is where the dependency injection
resolver is configured. For details on what **AddJ4JCommandLine()** does
see [this article](di.md). Note that I explicitly register **FancyConsole**. It's
the implementation I'm using of **IConsoleOutput**. **BindingTargetBuilder**
requires access to an implementation of **IConsoleOutput** to work. This
registration lets an instance of **BindingTargetBuilder** be retrieved from
the dependency injection resolver used in the **Main()** method.

The setup, binding and parsing code is very simple. First you create an
instance of **BindingTargetBuilder** and configure it (done here through
the **IServiceProvider** interface, powered by the **Autofac** dependency
injection system):

```
static void Main(string[] args)
{
    InitializeServiceProvider();

    var builder = _svcProvider.GetRequiredService<BindingTargetBuilder>();

    builder.Prefixes( "-", "--", "/" )
        .Quotes( '\'', '"' )
        .HelpKeys( "h", "?" )
        .Description( "a test program for exercising J4JCommandLine" )
        .ProgramName( $"{nameof(Program)}.exe" );

    // to be continued
```
Then create an instance of **BindingTarget<Configuration>** by calling the builder's
**Build()** method:

```
    // see above for details...

    if( !builder.Build<Configuration>(null, out var binder) )
        throw new NullReferenceException(nameof(Program));

    // to be continued
```
Next you bind the options to the public static properties:
```
    // see above for details...

    binder.Bind( x => x.IntValue, "i" )
        .SetDescription( "an integer value" )
        .SetDefaultValue( 1 )
        .SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

    binder.Bind( x => x.TextValue, "t" )
        .SetDescription( "a text value" )
        .SetDefaultValue( "some text value" );

    // to be continued
```
You don't have to set descriptions, default values or validators but that's how
you'd do it if you want to.

Finally, you parse the command line arguments supplied to **Main()**. Note that if
the parsing succeeds the updated **Configuration** object is available in
**binder.Value**. In a real example you'd want to assign it's value to some
field or property you can access wherever you need it:
```
    // see above for details...

    if( binder.Parse( args ) != MappingResults.Success )
    {
        Environment.ExitCode = 1;
        return;
    }

    Console.WriteLine($"IntValue is {binder.Value.IntValue}");
    Console.WriteLine($"TextValue is {binder.Value.TextValue}");
}
```
