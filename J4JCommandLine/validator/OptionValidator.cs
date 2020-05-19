using System;

namespace J4JSoftware.CommandLine
{
    public abstract class OptionValidator<TOption> : IOptionValidator<TOption>
    {
        public Type SupportedType => typeof(TOption);

        public abstract bool Validate( IBindingTarget bindingTarget, string key, TOption value );

        bool IOptionValidator.Validate( IBindingTarget bindingTarget, string key, object value )
        {
            if( value is TOption castValue )
                return Validate( bindingTarget, key, castValue );

            return true;
        }
    }
}