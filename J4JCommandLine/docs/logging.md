# Logging and Errors

Command lines, generally being user-supplied, are prone to causing all sorts of errors when they are parsed. Reporting those errors is complicated because most of the logging systems I'm familiar with get set up during an app's basic build/configuration phase, where logging isn't yet available.

I addressed these issues two ways in `J4JCommandLine`:

- It doesn't throw any exceptions and instead generally reports either success or failure. That report is ignored by the `IConfiguration` system but can be useful in tracking down problems during development.
- It logs unusual situations internally. Those log entries can be dumped to your app's logging system once the build phase is completed.

Dumping J4JCommandLine's log entries to your logging system is done by calling a single method and passing it an instance of Microsoft's `ILogger`:

```c#
// somewhere in your app after the build phase is completed
BuildTimeLoggerFactory.Default.Dump( yourLogger );
```

You can also restrict the level of log messages that get dumped by passing an optional second parameter to the `Dump()` call. By default, it dumps everything.
