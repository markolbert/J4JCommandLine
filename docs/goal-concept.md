### Goal and Concept

There are a number of C# command line parsers out there, several
of which I've used. So why write another one? 

Well, in part because, for me at least, I only really understand
a library after I've (re)written it. I was intrigued by some
of the design issues I learned about from Jon and wanted to
get a better feel for how to implement them.

But there was a bigger reason. I view C# as a type-centered
language. While it has many other characteristics at its core
everything seems to revolve around types. Because of that I
tend to thing of "parsing a command line" as "converting a
bunch of text into an instance of a configuration type". I 
wanted a library that was centered around types.

That's an ad hoc restriction. There are plenty of other targets
one could imagine turning command line text into (e.g., in C#,
fields or method variables). But it's a pretty powerful
and flexible restriction, I think, because once you have 
command line information in a type you can go anywhere you
want (e.g., assign information to method variables, call
methods defined in the configuration object using the
command line values, etc.).

I also wanted the library to work with dependency injection.
Initially that was a very pervasive design consideration but ultimately
it ended up being fairly trivial. The primary library has a related 
Autofac support library (I love Autofac -- it's worth checking out).

Finally, I wanted the library to be relatively easily extensible and
customizable. Consequently, most of its core revolve around key
interfaces. The guts of the code implement those interfaces and glue
them together into a functioning whole. But you should be able to 
customize the bits and just drop them into the overall framework (that's
dependency injection for you :)).

