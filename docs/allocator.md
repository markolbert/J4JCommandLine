### The First Stage Command Line Parser

**IAllocator** defines how the allocator -- the object that
allocates elements from the command line into a collection of text values 
keyed by option keys -- works. 

Given the sometimes oddball nature of what constitutes
a traditional command line in different environments you may want to
replace it with a custom one. This article describes the way the default
implementation works as a means of touching on issues you may need to 
consider in writing your own.

Conceptually the **AllocateCommandLine()** method is pretty simple. It reads 
characters one by one from the text it was given and decodes them into 
option keys and values. Unfortunately that is a somewhat complex process 
because the context of what a character means depends on the characters that 
have come before it.

Here's the code to the default **AllocateCommandLine()** implementation:
```
public IAllocations AllocateCommandLine( string cmdLine )
{
    var retVal = new Allocations(_keyComp);

    var accumulator = new StringBuilder();
    IAllocation? curResult = null;
    var charsProcessed = 0;
    var lastElementWasKey = false;

    for( var idx = 0; idx < cmdLine.Length; idx++)
    {
        accumulator.Append( cmdLine[idx] );
        charsProcessed++;

        var element = accumulator.ToString();

        // analyze the sequence as it currently stands to see if it includes
        // a prefixed key and/or has been terminated
        var maxPrefix = _prefixer.GetMaxPrefixLength(element);
        var maxTerminator = _terminator.GetMaxTerminatorLength(element, maxPrefix > 0);

        // keep adding characters unless we've encountered a termination 
        // sequence or we've reached the end of the command line
        if( maxTerminator <= 0 && charsProcessed < cmdLine.Length )
            continue;

        // extract the true element value from the prefix and terminator
        element = element[maxPrefix..^maxTerminator];

        // key values are identified by the presence of known prefixes
        if ( maxPrefix > 0 )
        {
            // because multiple key references are allowed (e.g., "-x abc -x def") check
            // to see if the key is already recorded. We only create and store a new
            // Allocation if the key isn't already stored
            if( !retVal.Contains( element ) )
                retVal.Add( new Allocation( retVal ) { Key = element } );

            // store a reference to the current/active Allocation unless the
            // element is a request for help (because help options cannot have
            // parameters)
            if( !_masterText.Contains( element, TextUsageType.HelpOptionKey ) )
            {
                curResult = retVal[ element ];
                lastElementWasKey = true;
            }
            else lastElementWasKey = false;
        }
        else
        {
            if( curResult == null || !lastElementWasKey )
                    retVal.Unkeyed.Parameters.Add( element );
            else curResult.Parameters.Add( element );

            lastElementWasKey = false;
        }

        // clear the accumulator so we can start processing the next character sequence
        accumulator.Clear();
    }

    return retVal;
}
```
Let's walk through a few key areas.
```
        var maxPrefix = Prefixer.GetMaxPrefixLength(element);
        var maxTerminator = _terminator.GetMaxTerminatorLength(element, maxPrefix > 0);

        // keep adding characters unless we've encountered a termination 
        // sequence or we've reached the end of the command line
        if( maxTerminator <= 0 && charsProcessed < cmdLine.Length )
            continue;
```
**maxPrefix** and **maxTerminator** are the character positions of the
"furthest" key prefix (e.g., "-", "--" or some such) and termination 
sequence (usually a single character). Those are looked for in
the variable **element**, which simply contains whatever is in the **StringBuilder** 
instance **accumulator**.

A **maxPrefix** of zero means no prefix has yet been found. A **maxTerminator**
of zero means no termination sequence has yet been found..but in deciding
whether or not to keep reading characters you have to honor the physical 
end of the command line, which isn't itself a character.
Once you've found a termination the first thing we do is strip away any
key prefixes and termination sequences (i.e., all we want from here on out
is the "pure" text value) from **element**.

```
        // extract the true element value from the prefix and terminator
        element = element[maxPrefix..^maxTerminator];
```
We then check to see if the element had an option key prefix (e.g., a "--"). 
If it does we check to see if we need a new **Allocation** object to hold 
parameter values. Since options can appear multiple times we don't want to 
create a new **Allocation** each time we find a key. We only create a 
new **Allocation** if there's isn't already one with that key in the 
**Allocations** collection we're building.

We also check to see if the **Allocation** we just created was for a 
help request. We only update the current **Allocation** -- which is
receiving text values -- if it's a non-help option.
```

        // key values are identified by the presence of known prefixes
        if ( maxPrefix > 0 )
        {
            // because multiple key references are allowed (e.g., "-x abc -x def") check
            // to see if the key is already recorded. We only create and store a new
            // Allocation if the key isn't already stored
            if( !retVal.Contains( element ) )
                retVal.Add( new Allocation( retVal ) { Key = element } );

            // store a reference to the current/active Allocation unless the
            // element is a request for help (because help options cannot have
            // parameters)
            if( !_masterText.Contains( element, TextUsageType.HelpOptionKey ) )
            {
                curResult = retVal[ element ];
                lastElementWasKey = true;
            }
            else lastElementWasKey = false;
        }
```
If the element wasn't a key it's just a text value that needs to be 
assigned to either the current **Allocation** or the global **Allocation**
representing "unkeyed" text values (which are plain old non-option command line
parameters).
```
        else
        {
            if( curResult == null || !lastElementWasKey )
                    retVal.Unkeyed.Parameters.Add( element );
            else curResult.Parameters.Add( element );

            lastElementWasKey = false;
        }

        // clear the accumulator so we can start processing the next character sequence
        accumulator.Clear();
```
