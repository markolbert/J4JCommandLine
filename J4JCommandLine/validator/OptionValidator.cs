using System;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public abstract class OptionValidator<TOption> : IOptionValidator<TOption>
    {
        protected OptionValidator()
        {
        }

        public abstract bool Validate( IBindingTarget bindingTarget, string key, TOption value );

        bool IOptionValidator.Validate(IBindingTarget bindingTarget, string key, object value)
        {
            if( value is TOption castValue )
                return Validate( bindingTarget, key, castValue );

            return true;
        }
    }
}