### Usage: Explicit Binding

With explicit binding you have to declare which properties are bound
to which properties in your startup code. Here's an example, from the 
StaticPropertyExample project:

```
using System;
using System.Collections.Generic;
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
            InitializeServiceProvider();

            var builder = ServiceProvider.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes( "-", "--", "/" )
                .Quotes( '\'', '"' )
                .HelpKeys( "h", "?" )
                .Description( "a test program for exercising J4JCommandLine" )
                .ProgramName( $"{nameof(Program)}.exe" );

            var binder = builder.Build<Program>(null);
            if (binder == null)
                throw new NullReferenceException( nameof(Program) );

            binder.Bind( x => Program.IntValue, "i" )
                .SetDescription( "an integer value" )
                .SetDefaultValue( 1 )
                .SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

            binder.Bind( x => Program.TextValue, "t" )
                .SetDescription( "a text value" )
                .SetDefaultValue( "some text value" );

            binder.BindUnkeyed( x => Program.Unkeyed );

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

All you do is invoke an instance of BindingTargetBuilder to create
a BindingTarget for your configuration type, bind properties of the
configuration type to option keys and call Parse() on the BindingTarget.
If the result is false there was a problem (and you should probably abort 
the app).

Along the way you can set option descriptions, default values and 
validators. The subsystem for displaying errors and help is defined
when the BindingTargetBuilder instance is created. It's a constructor
parameter, and in the example created on the fly by the Autofac
dependency injection framework.

There's a default/basic help/error display engine in the core library.
But there's also one that produces fancier output defined in the 
FancyHelpError project. Here's what the output looks like:

![Fancy Help output](assets/fancy-help.png)