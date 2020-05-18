using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.CompilerServices;

namespace J4JSoftware.CommandLine
{
    public interface IOption
    {
        Type SupportedType { get; }
        string Description { get; }
        List<string> Keys { get; }
        object DefaultValue { get; }
        bool IsRequired { get; }
        int MinParameters { get; }
        int MaxParameters { get; }
        IOptionValidator Validator { get; }

        //IOption SetDefaultValue( object defaultValue );
        //IOption SetDescription( string description );
        //IOption Required();
        //IOption Optional();
        //IOption AddKey(string key);
        //IOption AddKeys(IEnumerable<string> keys);
        //IOption SetValidator(IOptionValidator validator);

        bool Validate( IBindingTarget bindingTarget, string key, object value );

        IList CreateEmptyList();

        TextConversionResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out object result );

        TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out IList result);
    }

    //public interface IOption<TOption> : IOption
    //{
    //    bool Validate( IBindingTarget bindingTarget, string key, TOption value );

    //    TextConversionResult Convert(
    //        IBindingTarget bindingTarget,
    //        IParseResult parseResult,
    //        out TOption result );

    //    TextConversionResult ConvertList(
    //        IBindingTarget bindingTarget,
    //        IParseResult parseResult,
    //        out List<TOption> result);
    //}
}