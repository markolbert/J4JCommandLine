using System;

namespace J4JSoftware.CommandLine
{
    // describes the non-generic interface for validating an IOption's value
    public interface IOptionValidator
    {
        // the Type the validator can validate
        Type SupportedType { get; }

        bool Validate( IBindingTarget bindingTarget, string key, object value );
    }

    // the non-generic interface for validating an IOption's value
    public interface IOptionValidator<in TOption> : IOptionValidator
    {
        bool Validate( IBindingTarget bindingTarget, string key, TOption value );
    }
}