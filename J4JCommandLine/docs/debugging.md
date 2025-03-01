# Debugging

By default, `J4JCommandLine` grabs the command line from the operating system. However, there are occasions when you'll want to specify the command line explicitly. That's generally in tests.

To do that, you simply set the value of `CommandLineText` in the instance of `J4JCommandLineBuilder` you're using:

```c#
var optionBuilder = new J4JCommandLineBuilder( StringComparison.OrdinalIgnoreCase )
{
    CommandLineText = "whatever you want"
};
```

If `CommandLineText` holds any non-null or non-empty (i.e., anything other than whitespace), its value will be parsed, rather than the value provided by the operating system.
