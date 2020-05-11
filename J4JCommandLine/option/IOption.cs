using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.CompilerServices;

namespace J4JSoftware.CommandLine
{
    public enum TextConversionResult
    {
        Okay,
        FailedConversion,
        FailedValidation,
        ResultIsNull
    }

    public interface IOption
    {
        ReadOnlyCollection<string> Keys { get; }
        bool IsValid( object toCheck );
        TextConversionResult Convert( List<string>? textElements, out object result, out string? error );
    }

    public interface IOption<TOption> : IOption
    {
        TOption DefaultValue { get; }
        Func<TOption, bool>? Validator { get; }

        IOption<TOption> AddKey( string key );
        IOption<TOption> AddKeys( IEnumerable<string> keys );
        IOption<TOption> SetDefaultValue( TOption defaultValue );
        IOption<TOption> SetValidator( Func<TOption, bool> validator );
        bool IsValid( TOption toCheck );
        TextConversionResult Convert( List<string>? textElements, out TOption result, out string? error );
    }
}