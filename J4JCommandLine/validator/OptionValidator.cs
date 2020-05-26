using System;

namespace J4JSoftware.CommandLine
{
    // an abstract base class implementing IOptionValidator. Types derived from
    // this class can validate particular types
    public abstract class OptionValidator<TOption> : IOptionValidator<TOption>
    {
        // the Type the validator can validate
        public Type SupportedType => typeof(TOption);

        public abstract bool Validate( IBindingTarget bindingTarget, string key, TOption value );

        // allows validation in a type-agnostic way. Note that validating an unsupported Type
        // always returns true.
        bool IOptionValidator.Validate( IBindingTarget bindingTarget, string key, object value )
        {
            if( value is TOption castValue )
                return Validate( bindingTarget, key, castValue );

            return true;
        }
    }
}