### Dependency Injection

The constructor for **BindingTargetBuilder** requires a number of other objects
to work:

```
public BindingTargetBuilder(
    ICommandLineParser parser,
    IEnumerable<ITextConverter> converters,
    IConsoleOutput consoleOutput
)
```

You could create them by hand...but they each depend on other objects being
created, which makes manual creation tedious.

Dependency injection can make this a *lot* simpler. 

There are a number of dependency injection frameworks available for C# and Net 
Core. Everyone has their favorite. Mine is [Autofac](https://autofac.org) which is
why I built the add-on library **AutofaceCommandLine**.

It offers a handful of extension methods to make setting up **J4JCommandLine** more
convenient:

- **AddJ4JCommandLine()**, which sets everything to typical defaults:
  - registers the default **IKeyPrefixer** that determines whether a character 
sequence is the prefix to an option key, e.g., the "--" in "--x");
  - registers the default **IElementTerminator** that determines when an 
option or a value has ended on the command line;
  - the default **ICommandLineParser** that converts the command line arguments 
into a collection of text values keyed by option keys;
  - **BindingTargetBuilder** itself; and,
  - all the **ITextConverter** classes in the framework's primary assembly.
- **AddTextConverters( this ContainerBuilder builder, Assembly toScan )**, which
registers all the **ITextConverter** implementations in the specified assembly
- **AddTextConverters(this ContainerBuilder builder, params Type[] textConverters )**,
which registers the explicitly-provided types implementing **ITextConverter**. Note
that any supplied types which *don't* implement **ITextConverter** will simply be
ignored and not registered.

```
public static ContainerBuilder AddJ4JCommandLine( this ContainerBuilder builder )
{
    builder.RegisterType<KeyPrefixer>()
        .AsImplementedInterfaces();

    builder.RegisterType<ElementTerminator>()
        .AsImplementedInterfaces();

    builder.RegisterType<CommandLineParser>()
        .AsImplementedInterfaces();

    builder.RegisterType<BindingTargetBuilder>()
        .AsSelf();

    builder.AddTextConverters( typeof(ITextConverter).Assembly );

    return builder;
}

public static ContainerBuilder AddTextConverters( this ContainerBuilder builder, Assembly toScan )
{
    builder.RegisterAssemblyTypes(toScan)
        .Where(t => !t.IsAbstract
                    && typeof(ITextConverter).IsAssignableFrom(t)
                    && t.GetConstructors().Length > 0)
        .AsImplementedInterfaces();

return builder;
}

public static ContainerBuilder AddTextConverters(this ContainerBuilder builder, params Type[] textConverters )
{
    foreach( var textConv in textConverters )
    {
        if( textConv.IsAbstract
            || !typeof(ITextConverter).IsAssignableFrom( textConv )
            || textConv.GetConstructors().Length == 0 )
            continue;

        builder.RegisterType( textConv )
            .AsImplementedInterfaces();
    }

    return builder;
}
```