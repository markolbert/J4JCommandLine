### Usage: Automatic Binding

Automatic binding has the framework discover the properties you want to
bind to options. You indicate the bound properties by decorating them 
with an `OptionKeys` attribute. Either static or instance properties can
be used, provided they are publicly read/write and there is an ITextConverter
type defined in the framework which supports their type.

There are other attributes you can use to provide additional information 
to the framework, to configure the bound options the way you want.

Here's an example, from the AutoBindExample project:

```
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBindExample
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeServiceProvider();

            var builder = ServiceProvider.GetRequiredService<BindingTargetBuilder>();

            builder.Prefixes("-", "--", "/")
                .Quotes('\'', '"')
                .HelpKeys("h", "?")
                .Description("a test program for exercising J4JCommandLine")
                .ProgramName($"{nameof(Program)}.exe");

            var binder = builder.AutoBind<Program>();
            if (binder == null)
                throw new NullReferenceException(nameof(Program));

            var intOption = binder.Options[ "i" ];
            if( intOption != null )
                intOption.SetValidator( OptionInRange<int>.GreaterThan( 0 ) );

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
        }

        public static IServiceProvider ServiceProvider { get; set; }

        [OptionKeys("i")]
        [DefaultValue(5)]
        [Description("some integer value")]
        public static int IntValue { get; set; }

        [OptionKeys("t")]
        [DefaultValue("abc")]
        [Description("some text value")]
        public static string TextValue { get; set; }

        [OptionKeys()]
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

All you do is create an instance of `BindingTargetBuilder` and then
call `AutoBind<>` on it.

The attributes you can use to configure the bindings are:

| Attribute | Capability | Comments |
| --------- | ---------- | -------- |
| OptionKeys | defines the key(s) for the binding | **required** |
| OptionRequired | makes the binding required or optional | optional |
| DefaultValue | specifies a default value | optional |
| Description | supplies a description | optional |

**Note**: *Because option validators are derived from a generic abstract
base class you cannot specify validators through attributes*.

Instead, you must retrieve the option you want to validate from the
binder's Options collection and then specify a validator for it. In the 
example this is done in the following lines:
```
var intOption = binder.Options[ "i" ];
if( intOption != null )
    intOption.SetValidator( OptionInRange<int>.GreaterThan( 0 ) );
```

There's a default/basic help/error display engine in the core library.
But there's also one that produces fancier output defined in the 
FancyHelpError project. Here's what the output looks like:

![Fancy Help output](assets/fancy-help.png)