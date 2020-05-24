using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IOption
    {
        ITargetableType TargetableType { get; }
        string Description { get; }
        string FirstKey { get; }
        List<string> Keys { get; }
        object? DefaultValue { get; }
        OptionType OptionType { get; }
        bool IsRequired { get; }
        int MinParameters { get; }
        int MaxParameters { get; }
        IOptionValidator Validator { get; }

        bool Validate( IBindingTarget bindingTarget, string key, object value );

        //IList CreateEmptyList();
        //Array CreateEmptyArray( int capacity );

        MappingResults Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            ITargetableType targetType,
            out object? result );

        //TextConversionResult ConvertList(
        //    IBindingTarget bindingTarget,
        //    IParseResult parseResult,
        //    out IList result );
    }
}