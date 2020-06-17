### Static Property Example

The most likely place you'd use a command line parser is in a console
app. C# console apps are typically defined via static methods (e.g., in a
**Program** class) so, for convenience, the framework is able to bind to
a class' public static properties.

Here's an example of such a class/app (the source code is in 
examples/StaticPropertyExample). It's using [Autofac](https://autofac.org) as
the dependency injection system and the extension library *AutofacCommandLine*
that's available in the repository.

```
using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleAppJ4JCmdLine
{
    public class Program
    {
        static void Main(string[] args)
        {
            // detail follows below, omitted here for brevity...
        }

        // this next property is here just for convenience; it's not
        // a binding target (and couldn't be as IServiceProvider
        // is not a targetable type)
        public static IServiceProvider ServiceProvider { get; set; }
        
        public static int IntValue { get; set; }
        public static string TextValue { get; set; }
        public static List<string> Unkeyed { get; set; }

        private static void InitializeServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FancyConsole>()
                .AsImplementedInterfaces();

            builder.AddJ4JCommandLine();

            ServiceProvider = new AutofacServiceProvider(builder.Build());
        }
    }
}
```

Two of the public static properties, **IntValue** and **TextValue** are bound
to command line options in code I'll show below. The third property, **Unkeyed**,
will be bound to any "unkeyed" options -- plain old command line parameters --
found on the command line. Otherwise there's nothing
special about them. They're just regular static properties with public get
and set accessors.

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

    var builder = ServiceProvider.GetRequiredService<BindingTargetBuilder>();

    builder.Prefixes( "-", "--", "/" )
        .Quotes( '\'', '"' )
        .HelpKeys( "h", "?" )
        .Description( "a test program for exercising J4JCommandLine" )
        .ProgramName( $"{nameof(Program)}.exe" );

    // to be continued
```
Then create an instance of **BindingTarget<Program>** by calling the builder's
**Build()** method, checking to make sure it's not null:

```
    // see above for details...

    var binder = builder.Build<Configuration>( null );
    if( binder == null )
        throw new NullReferenceException(nameof(Program));

    // to be continued
```
Next you bind the options to the public static properties:
```
    // see above for details...

    binder.Bind( x => Program.IntValue, "i" )
        .SetDescription( "an integer value" )
        .SetDefaultValue( 1 )
        .SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

    binder.Bind( x => Program.TextValue, "t" )
        .SetDescription( "a text value" )
        .SetDefaultValue( "some text value" );

    binder.BindUnkeyed( x => Program.Unkeyed );

    // to be continued
```
You don't have to set descriptions, default values or validators but that's how
you'd do it if you want to.

Finally, you parse the command line arguments supplied to **Main()**:
```
    // see above for details...

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
}
```
