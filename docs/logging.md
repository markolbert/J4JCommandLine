### Logging and Errors

Command lines, generally being user-supplied, are prone to causing all sorts of 
errors when they are parsed. Reporting those errors is complicated because
most of the logging systems I'm familiar with are set up using dependency injection
and that's generally not yet available when you're parsing command lines (that
appears to be the case with the Net5 `IHostBuilder` system).

It's further complicated when you're feeding into the `IConfiguration` system by
the nature of that system: it merges configuration information from a variety of
sources and, if it can't get the answer it wants from the command line stuff but
can get it from something else (e.g., environment variables, configuration files)
it'll do that and not throw an exception.

I addressed these issues two ways in `J4JCommandLine`:
- It throws relatively few exceptions and instead generally reports either 
success or failure. That report is ignored by the `IConfiguration` system but can 
be useful in tracking down problems during development.
- It optionally supports logging (through a companion library, `J4JLogger`).

You still have to deal with the chicken-and-egg problem to use logging. But the
`J4JLogger` library provides a caching logger which you can use to accumulate
log messages from `J4JCommandLine` during your app's startup/configuration phase and
then dump those cached messages into the logging pipeline once it's set up.

Here are the exceptions `J4JCommandLine` may throw:
- an **InvalidEnumArgumentException** if I add a new `OptionStyle` to the library 
and forget to update the rest of the code base.
- an **InvalidEnumArgumentException** because, again, I added a new 
`CommandLineStyle` but forgot to update the rest of the code base.
- an **InvalidEnumArgumentException** because *Microsoft* added a new 
`StringComparison` but I failed to update the code base.