### Command Line Styles
Configuring `J4JCommandLine` requires that you choose option prefixes (e.g., the
'-' or '--' used in command lines), *quoters* (e.g., the '"' used to delimit
things separated by what is normally a separator, like "this is all one argument")
and the like.

You can do that manually by declaring an instance of `MasterTextCollection` and
configuring it. Or you can use the built-in defaults by calling
`MasterTextCollection.GetDefault()` specifying a particular `CommandLineStyle`.

Here are the defaults it defines:
```csharp
switch( cmdLineStyle )
{
    case CommandLineStyle.Linux:
        retVal.AddRange( TextUsageType.Prefix, "-", "--" );
        retVal.AddRange( TextUsageType.Quote, "\"", "'" );
        retVal.Add( TextUsageType.ValueEncloser, "=" );
        retVal.AddRange( TextUsageType.Separator, " ", "\t" );

        return retVal;

    case CommandLineStyle.Universal:
        retVal.AddRange( TextUsageType.Prefix, "-", "--", "/" );
        retVal.AddRange( TextUsageType.Quote, "\"", "'" );
        retVal.Add( TextUsageType.ValueEncloser, "=" );
        retVal.AddRange( TextUsageType.Separator, " ", "\t" );

        return retVal;

    case CommandLineStyle.Windows:
        retVal.AddRange( TextUsageType.Prefix, "/", "-", "--" );
        retVal.Add( TextUsageType.Quote, "\"" );
        retVal.Add( TextUsageType.ValueEncloser, "=" );
        retVal.AddRange( TextUsageType.Separator, " ", "\t" );

        return retVal;
}
```
**When creating an `OptionCollection`, if you don't specify a `CommandLineStyle` the
library assumes you want `CommandLineStyle.Windows`.**