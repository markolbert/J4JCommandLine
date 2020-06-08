### The First Stage Command Line Parser

**ICommandLineParser** defines how the first-stage parser -- the one that
parses the raw command line into a collection of text values keyed by
option keys -- works. Given the sometimes oddball nature of what constitutes
a traditional command line in different environments you may want to
replace it with a custom one. This article describes the way the default
implementation works as a means of touching on issues you may need to 
consider in writing your own.

Conceptually the **Parse()** method is pretty simple. It reads characters
one by one from the text it was given and decodes them into option keys 
and values. Unfortunately that is a somewhat complex process because
the context of what a character means depends on the characters that have
come before it.

Here's the code to the default **Parse()** implementation:
```
public ParseResults Parse( string cmdLine )
{
    var retVal = new ParseResults(_keyComp);

    var accumulator = new StringBuilder();
    IParseResult? curResult = null;
    var lastElementWasKey = false;
    var charsProcessed = 0;

    for( var idx = 0; idx < cmdLine.Length; idx++)
    {
        accumulator.Append( cmdLine[idx] );
        charsProcessed++;

        var element = accumulator.ToString();

        // analyze the sequence as it currently stands to see if it includes
        // a prefixed key and/or has been terminated
        var maxPrefix = Prefixer.GetMaxPrefixLength(element);
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
            // if the prior element was a keyvalue then it must've
            // been for a switch (parameterless) option so add a "true"
            // parameter to it to complete it
            if( lastElementWasKey )
                curResult?.Parameters.Add( "true" );

            lastElementWasKey = true;

            // because multiple key references are allowed (e.g., "-x abc -x def") check
            // to see if the key is already recorded. We only create and store a new
            // ParseResult if the key isn't already stored
            if( !retVal.Contains( element ) )
            {
                var newResult = new ParseResult{Key = element};

                // if we're adding an new option/key at the end of the command line
                // it must be a switch (parameterless option), so add "true" to its
                // parameters
                if( idx == ( cmdLine.Length - 1 ) )
                    newResult.Parameters.Add( "true" );

                retVal.Add( newResult );
            }

            // store a reference to the current/active ParseResult
            curResult = retVal[ element ];
        }
        else
        {
            lastElementWasKey = false;

            // since the element is an option value store the text as a 
            // parameter. We store it into curResult because curResult is the
            // instance associated with the last key value
            // if curResult is null it's because we haven't encountered our first
            // key, in which case we just ignore the parsed text
            curResult?.Parameters.Add( element );
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
```
        // extract the true element value from the prefix and terminator
        element = element[maxPrefix..^maxTerminator];

        // key values are identified by the presence of known prefixes
        if ( maxPrefix > 0 )
        {
            // if the prior element was a keyvalue then it must've
            // been for a switch (parameterless) option so add a "true"
            // parameter to it to complete it
            if( lastElementWasKey )
                curResult?.Parameters.Add( "true" );

            lastElementWasKey = true;
```
Once you've found a termination the first thing we do is strip away any
key prefixes and termination sequences (i.e., all we want from here on out
is the "pure" text value) from **element**.

We then check to see if the element had an option key prefix (e.g., a "--"). 
Normally the fact you've found a key prefix means you should simply 
create a new **ParseResult** object to hold the key value and the following 
text values. But switch options (e.g., the 'x' in "-x -a 27") don't 
*have* values...so if you find one a switch option you add a "true" text 
value to the *last* **ParseResult**'s set of text values.
```
            // because multiple key references are allowed (e.g., "-x abc -x def") check
            // to see if the key is already recorded. We only create and store a new
            // ParseResult if the key isn't already stored
            if( !retVal.Contains( element ) )
            {
                var newResult = new ParseResult{Key = element};

                // if we're adding an new option/key at the end of the command line
                // it must be a switch (parameterless option), so add "true" to its
                // parameters
                if( idx == ( cmdLine.Length - 1 ) )
                    newResult.Parameters.Add( "true" );

                retVal.Add( newResult );
            }

            // store a reference to the current/active ParseResult
            curResult = retVal[ element ];
```
Since options can appear multiple times we don't want to create a new
**ParseResult** each time we find a key. We only create a new **ParseResult**
if there's isn't already one with that key in the collection we're building.
We then store the **ParseResult** we're working on in the variable
**curResult** for convenience.
```
        }
        else
        {
            lastElementWasKey = false;

            // since the element is an option value store the text as a 
            // parameter. We store it into curResult because curResult is the
            // instance associated with the last key value
            // if curResult is null it's because we haven't encountered our first
            // key, in which case we just ignore the parsed text
            curResult?.Parameters.Add( element );
        }

        // clear the accumulator so we can start processing the next character sequence
        accumulator.Clear();
```
If **element** was *not* an option key we store that status for the next
go-round and add **element** to the text values of our most recent
**curResult**. Note that if **curResult** is not yet defined nothing will
be done (i.e., the value of **element** will be discarded). In practice that
means everything before the first command line option is discarded.