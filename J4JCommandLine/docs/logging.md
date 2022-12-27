# Logging and Errors

Command lines, generally being user-supplied, are prone to causing all sorts of errors when they are parsed. Reporting those errors is complicated because most of the logging systems I'm familiar with are set up using dependency injection and that's generally not yet available when you're parsing command lines.

It's further complicated when you're feeding into the `IConfiguration` system by the nature of that system: it merges configuration information from a variety of sources and, if it can't get the answer it wants from the command line stuff but can get it from something else (e.g., environment variables, configuration files) it'll do that.

I addressed these issues two ways in `J4JCommandLine`:

- It throws relatively few exceptions and instead generally reports either success or failure. That report is ignored by the `IConfiguration` system but can be useful in tracking down problems during development.
- It optionally supports logging (through a companion library, [J4JLogger](https://github.com/markolbert/J4JLogging)).

You still have to deal with the chicken-and-egg problem to use logging. But the `J4JLogger` library provides a caching logger which you can use to accumulatelog messages from `J4JCommandLine` during your app's startup/configuration phase and then dump those cached messages into the logging pipeline once it's set up.

Here are the exceptions `J4JCommandLine` may throw:

|Context|Exception|When Triggered|
|-------|---------|--------------|
|`J4JCommandLineProvider::Load()`|`InvalidEnumArgumentException`|An option implements an unsupported `OptionStyle`. This should only happen if I add more styles and forget to update the rest of the library|
|`::AddJ4JCommandLineForWindows()`, `::AddJ4JCommandLineForLinux()`|`InvalidEnumArgumentException`|An unsupported `CommandLineOperatingSystems` value is encountered. This should only happen if I add more operating systems and forget to update the rest of the library|
|`HelpDisplay::GetStyleText()`|`InvalidEnumArgumentException`|An option implements an unsupported `OptionStyle`. This should only happen if I add more styles and forget to update the rest of the library|
|`TextConverters::[Type]`|`KeyNotFoundException`|an `ITextToValue` converter is requested for an unsupported `Type`|
|`Option::MaxValues`, `Option::ValuesSatisfied`|`InvalidEnumArgumentException`|An option implements an unsupported `OptionStyle`. This should only happen if I add more styles and forget to update the rest of the library|
