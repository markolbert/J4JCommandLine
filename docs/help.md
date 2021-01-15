### Outputting Help

`J4JCommandLine` does not automatically output help information when
a particular key exists in the command line that's being parsed. That's
because it's intended to be used as part of the Net5 `IConfiguration`
system, which can consolidate information from a wide variety of sources.

Instead, you force help information to be displayed by calling the
`DisplayHelp()` method on an instance of `IOptionCollection`:
```csharp
options.DisplayHelp();
```
Calling `DisplayHelp()` without any arguments causes help information
to be displayed using the default help formatter. This produces a very
simple -- and not remotely typical -- display:
![simple help](assets/simple-help.png)
The simplicity results from me not wanting to invest the time developing
code which could format help information in a more typical fashion. As 
a result I doubt anyone will rely on the default formatter.
#### Colorful Help
Instead, you should write your own formatter, or include the add-on 
assembly `ColorfulHelp` and use it's `DisplayColorHelp` formatter:
```csharp
options.DisplayHelp( new DisplayColorHelp() );
```
That gets you a display which looks like this:
![fancy help](assets/fancy-help.png)

`DisplayColorHelp` offers a few properties you can use to tweak
the display:
- **CellPadding**, which lets you control the padding within each
cell of the table. The default is two spaces horizontally and no spaces
vertically.
- **HeadingColor**, which lets you specify the color of the column
headers. The default is *green*.
- **TitleColor**, which is used for the help title. The default is
*yellow*.
- **EmphasisColor**, which is used for emphasizing text such as keys. 
The default is *cyan*.
- **TextColor**, which is used for basic text. The default is *white*.
- **GridColor**, which is used for the table grid. The default is *gray*.

#### Implementing Your Own Help Formatter
Implementing your own help formatter simply involves creating a class
which implements the `IDisplayHelp` interface:
```csharp
public interface IDisplayHelp
{
    void ProcessOptions( IOptionCollection options );
}
```
To make that easier there's a base class you can extend, which also
offers a couple of helper methods, `DisplayHelpBase`:
```csharp
public abstract class DisplayHelpBase : IDisplayHelp
{
    protected DisplayHelpBase( IJ4JLogger? logger )
    {
        Logger = logger;
        Logger?.SetLoggedType( GetType() );
    }

    protected IJ4JLogger? Logger { get; }

    public abstract void ProcessOptions( IOptionCollection options );

    protected List<string> GetKeys( IOption option )
    {
        // details omitted; merges key prefixes and keys to
        // create output that looks like the keys a user would 
        // enter on the command line
    }

    protected string GetStyleText( IOption option )
    {
        // details omitted; uses information from an option's
        // Style and Required properties to create information about
        // about how many values can be specified, whether a value
        // is required, etc.
    }
}
```