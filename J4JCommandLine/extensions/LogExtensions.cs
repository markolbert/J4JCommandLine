using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

internal static partial class LogExtensions
{
    #region options

    [ LoggerMessage( LogLevel.Error,
                     "{caller}: trying to commit a value to a {valueText} value to a {optionText} option" ) ]
    public static partial void InvalidAssignment(
        this ILogger logger,
        string valueText,
        string optionText,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: Invalid {prop} option value '{value}'" ) ]
    public static partial void InvalidOptionValue(
        this ILogger logger,
        string prop,
        string value,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: Binding target {target} is not a property" ) ]
    public static partial void InvalidBindingTarget(
        this ILogger logger,
        string target,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: Binding target {target} could not be bound by {binder}" ) ]
    public static partial void BindingFailed(
        this ILogger logger,
        string target,
        string binder,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: duplicate binding for property path {path}" ) ]
    public static partial void DuplicateBinding(
        this ILogger logger,
        string path,
        [ CallerMemberName ] string caller = ""
    );

    #endregion

    #region text converters

    [ LoggerMessage( LogLevel.Error, "{caller}: duplicate converter defined for {type}" ) ]
    public static partial void DuplicateConverter(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: no converter defined for {type}" ) ]
    public static partial void MissingConverter(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Warning, "{caller}: no built-in converters defined" ) ]
    public static partial void MissingBuiltInConverters(
        this ILogger logger,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: could not convert text to {type}" ) ]
    public static partial void TextConversionFailed(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: could not convert '{value}' to {type}" ) ]
    public static partial void ConversionFailed(
        this ILogger logger,
        string type,
        string value,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: cannot convert multiple text values to a single {type}" ) ]
    public static partial void TooManyValues(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    #endregion

    [ LoggerMessage( LogLevel.Error, "{caller}: Unclosed quoter encountered, command line truncated at token #{idx}" ) ]
    public static partial void UnclosedQuotes( this ILogger logger, int idx, [ CallerMemberName ] string caller = "" );

    [ LoggerMessage( LogLevel.Error, "{caller}: undefined parsing action for token sequence {type1} => {type2}" ) ]
    public static partial void UndefinedParsingAction(
        this ILogger logger,
        string type1,
        string type2,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: failed to create instance of {type}" ) ]
    public static partial void FailedToCreateInstance(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: cannot include {type} tokens" ) ]
    public static partial void InvalidTokens(
        this ILogger logger,
        string type,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: duplicate token text '{text}' for {type}" ) ]
    public static partial void DuplicateTokenText(
        this ILogger logger,
        string type,
        string text,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error, "{caller}: unsupported operating system {os}" ) ]
    public static partial void UnsupportedOs( this ILogger logger, string os, [ CallerMemberName ] string caller = "" );

    [ LoggerMessage( LogLevel.Warning, "{caller}: unsupported text comparison, defaulting to OrdinalIgnoreCase" ) ]
    public static partial void UnsupportedTextComparison(
        this ILogger logger,
        [ CallerMemberName ] string caller = ""
    );

    [ LoggerMessage( LogLevel.Error,
                     "{caller}: unsupported operating system, lexical elements (i.e., tokens) must be supplied" ) ]
    public static partial void UndefinedLexicalElements( this ILogger logger, [ CallerMemberName ] string caller = "" );

    [LoggerMessage(LogLevel.Error, "{caller}: unsupported option style {style}, ignoring")]
    public static partial void UnsupportedOptionStyle(
        this ILogger logger,
        string style,
        [ CallerMemberName ] string caller = ""
    );
}
