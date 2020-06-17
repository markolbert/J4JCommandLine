### Terminology

One of the things I learned as I wrote this library is "parsing a
command line" involves steps that aren't what I tend to think of as
parsing per se. So to aid in understanding the documentation here are
the definitions of some terms I'm using.

In this library the first step in processing command line arguments 
involves **allocating** them to **allocations**. Consider the following
command line:
```
-x 1 -y abc -s def
```
- **x** is bound to an integer property. It should end up with the value 1.
- **y** is bound to a string property. It should end up with the value "abc".
- **s** is a *switch*, bound to a boolean property. It should be true.
- **def** is an **unkeyed option**, otherwise known as a plain-vanilla command
line argument.

For now we'll ignore how the binding takes place (it's simple) because 
we're focusing on how to process the command line.

The `IAllocator` interface defines the step of allocating the elements of
the command line to allocations. Which are then returned as an
`IAllocations` collection:
```
public interface IAllocator
{
    bool IsInitialized { get; }

    bool Initialize(
        StringComparison keyComp,
        CommandLineLogger logger,
        MasterTextCollection masterText );
        
    Allocations AllocateCommandLine( string[] args );
    Allocations AllocateCommandLine( string cmdLine );
}
```
An `IAllocation` object contains the **key** identifying an allocation
(e.g., the **x** in "-x") and the parameters that *may* be associated with
the key. I say *may* because, since the allocator doesn't know whether
a particular key corresponds to a switch or an option requiring a parameter 
it has to guess where to stash some parameters. Moving parameters around
is taken care of later, in the **parsing** step.

Here's what `Allocator` produces for the example command line:

| Portion of Command Line | Allocation Key | Allocation Parameters |
| ----------------------- | -------------- | --------------------- |
| -x 1 | x | 1 |
| -y abc | y | abc |
| -s def | s | def |

The last result -- for the **s** key -- is incorrect because **s** is a 
switch option, bound to a boolean. But the allocator can't know that
because it's not concerned with or aware of the bindings. So it
allocates **def** to the **s** key.

This misallocation gets handled in the next processing phase where the
allocations are **parsed** and assigned to the properties they are bound
to. Validation also takes place in the parsing phase, as does the 
assignment of any default values which may be associated with a particular
binding.
