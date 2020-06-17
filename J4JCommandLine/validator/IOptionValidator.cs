using System;

namespace J4JSoftware.CommandLine
{
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
}