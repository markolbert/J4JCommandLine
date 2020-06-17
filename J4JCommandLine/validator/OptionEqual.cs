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

        public override bool Validate( Option option, T value, CommandLineLogger logger )
        {
            if( value.Equals( _checkValue ) )
                return true;

            logger.LogError( ProcessingPhase.Validating, $"'{value}' does not equal '{_checkValue}'", option : option );

            return false;
        }
    }
}