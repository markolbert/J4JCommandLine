using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.CompilerServices;

namespace J4JSoftware.CommandLine
{
    public interface IOption
    {
        ReadOnlyCollection<string> Keys { get; }
        bool Validate( IBindingTarget bindingTarget, string key, object value );

        TextConversionResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out object result );

        TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out List<object> result);
    }

    public interface IOption<TOption> : IOption
    {
        TOption DefaultValue { get; }

        IOption<TOption> AddKey( string key );
        IOption<TOption> AddKeys( IEnumerable<string> keys );
        IOption<TOption> SetDefaultValue( TOption defaultValue );
        IOption<TOption> SetValidator( IOptionValidator<TOption> validator );
        bool Validate( IBindingTarget bindingTarget, string key, TOption value );

        TextConversionResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out TOption result );

        TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out List<TOption> result);
    }
}