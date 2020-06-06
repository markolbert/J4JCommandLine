using System;

namespace J4JSoftware.CommandLine
{
    // determines whether two instances are equal by using the Type's IEquatable<T> interface.
    // Consequently, only Types supporting that interface can be validated by this class.
    public class OptionEqual<T> : OptionValidator<T>
        where T : IEquatable<T>
    {
        private readonly T _checkValue;

        public OptionEqual( T checkValue )
        {
            _checkValue = checkValue;
        }

        public override bool Validate( IBindingTarget bindingTarget, string key, T value )
        {
            if( value.Equals( _checkValue ) )
                return true;

            bindingTarget.AddError( key, $"'{value}' does not equal '{_checkValue}'" );

            return false;
        }
    }
}