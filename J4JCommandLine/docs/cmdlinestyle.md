# Command Line Styles

Configuring `J4JCommandLine` requires you choose option prefixes (e.g., the '-' or '--' used in command lines), *quoters* (e.g., the '"' used to delimit things separated by what is normally a separator, like "this is all one argument") and the like. I refer to these as lexical elements.

You can do that manually by declaring an instance of `LexicalElements` and configuring it. Or you can use one of the built-in defaults by creating instances of `WindowsLexcialElements` or `LinuxLexicalElements`.
