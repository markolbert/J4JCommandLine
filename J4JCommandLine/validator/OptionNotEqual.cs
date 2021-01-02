using System;

namespace J4JSoftware.CommandLine.Deprecated
{
    // determines whether two instances are not equal by using the Type's IEquatable<T> interface.
    // Consequently, only Types supporting that interface can be validated by this class.
    public class OptionNotEqual<T> : OptionValidator<T>
        where T : IEquatable<T>
    {
        private readonly T _checkValue;

        public OptionNotEqual( T checkValue )
        {
            _checkValue = checkValue;
        }

        public override bool Validate( Option option, T value, CommandLineLogger logger )
        {
            if( !value.Equals( _checkValue ) )
                return true;

            logger.LogError( ProcessingPhase.Validating, $"'{value}' equals '{_checkValue}'", option : option );

            return false;
        }
    }
}