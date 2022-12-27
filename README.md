# J4JCommandLine

A Net library which adds parsing command line arguments to the `IConfiguration` system. Command line arguments can be bound to instance or static properties of classes, including nested classes.

There are some restrictions on the nature of the target classes, mostly having to do with them having public parameterless constructors. This is the same constraint as exists for the `IConfiguration` system.

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.Configuration.CommandLine?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.Configuration.CommandLine/)

The libraries are licensed under the GNU GPL-v3 or later. For more details see the [license file](LICENSE.md).

See the [documentation](J4JCommandLine/docs/readme.md) for details.

## ColorfulHelp

A `J4JCommandLine` extension library which adds colorfully formatted output when displaying help screens.

[![Nuget](https://img.shields.io/nuget/v/J4JSoftware.Configuration.CommandLine?style=flat-square)](https://www.nuget.org/packages/J4JSoftware.Configuration.CommandLine/)

The libraries are licensed under the GNU GPL-v3 or later. For more details see the [license file](LICENSE.md).

See the [documentation](ColorfulHelp/docs/readme.md) for details.

## Inspiration and Dedication

This library is dedicated to Jon Sequitur, one of the leads on Microsoft's own command line parser, for his patient explanations of how that other parser is designed and his suggestions for potential changes to it which eventually led to me writing `J4JCommandLine`.
