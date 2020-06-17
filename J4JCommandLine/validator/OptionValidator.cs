using System;

namespace J4JSoftware.CommandLine
{
    // an abstract base class implementing IOptionValidator. Types derived from
    // this class can validate particular types
    public abstract class OptionValidator<T> : IOptionValidator<T>
    {
        // the Type the validator can validate
        public Type SupportedType => typeof(T);

        public abstract bool Validate( Option option, T value, CommandLineLogger logger );

        // allows validation in a type-agnostic way. Note that validating an unsupported Type
        // always returns true.
        bool IOptionValidator.Validate( Option option, object value, CommandLineLogger logger )
        {
            if( value is T castValue )
                return Validate( option, castValue, logger );

            return true;
        }
    }
}