using System;
using System.Collections.Generic;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public interface IOptionValidator
    {
        Type SupportedType { get; }
        bool Validate( IBindingTarget bindingTarget, string key, object value );
    }

    public interface IOptionValidator<in TOption> : IOptionValidator
    {
        bool Validate( IBindingTarget bindingTarget, string key, TOption value );
    }
}
