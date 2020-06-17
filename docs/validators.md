### Adding Validators

All framework validators have to implement the `IValidator<in T>` interface,
which in turn requires implementation of the `IValidator` interface:
```
// describes the non-generic interface for validating an IOption's value
public interface IOptionValidator
{
    // the Type the validator can validate
    Type SupportedType { get; }

    bool Validate( Option option, object value, CommandLineLogger logger );
}

// the non-generic interface for validating an IOption's value
public interface IOptionValidator<in T> : IOptionValidator
{
    bool Validate( Option option, T value, CommandLineLogger logger );
}
```
For convenience there's an abstract base class, `OptionValidator` you can 
use to create validators:
```
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
```
The base class takes care of implementing the type-agnostic `Validate()`
method.

As an example, here's the implementation for the `OptionEqual` validator:
```
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
```
It constrains the type being validated to one which implements the 
`IEquatable<T>` interface (since we're testing for equality :)).