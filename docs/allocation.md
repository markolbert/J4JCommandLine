### Allocation

Allocation is what I call the first stage of parsing. It's where the various
text values on the command line are allocated/assigned to collections
keyed by option keys (and to one unkeyed collection). For details on how
that works please refer to [this article](allocator.md).

This article focuses on what a command line could look like an be 
allocated, and then parsed, successfully. To do so it helps to distinguish
between options -- what the framework refers to as keyed options -- and
plain old command line parameters.

For a piece of text to be considered a (keyed) option it must be preceded by
an **option key**, which is composed of two parts: an allowed prefix (e.g., -, --
or /) and some text, terminated by a terminator character. 

Spaces are always terminator characters unless they occur inside a piece of
quoted text. So the space in `-x 1` terminates the option key while the
second and third spaces in `-x "some quoted text"` are part of the text 
element's value and don't terminate anything.

Every option must be preceded by an option key. Conversely, any text element
not preceded by an option key is considered merely a value to be assigned
to either a keyed option collection or the overall unkeyed option collection.
There's only one unkeyed option collection for each parsing action.

During the allocation phase all the parameters associated with the same
option key are stored in the same collection. Multiple values do not need
to be specified sequentially but each must be preceded by the same option
key.

For example, the allocator converts `-x abc -y -x def` into two collections,
one keyed by **x** containing two values *abc* and *def* and one keyed by
**y** containing no values (**y** is presumably a switch option).

However, while the allocator converts `-x abc def -y` into the same two
collections, the **x** collection in the second example *only* contains
*abc*. The *def* parameter value is stored in the **unkeyed option collection**.
That latter collection is where what are traditionally thought
of as plain old command line parameters end up. They can be converted
in the parsing phase to a collection of any type for which a text converter 
is defined.