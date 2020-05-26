using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // Defines the interface for an Option which can be bound to a TargetProperty
    public interface IOption
    {
        // Information about the targetability of the type of the TargetProperty to
        // which the option is bound. Options can be bound to un-targetable properties.
        ITargetableType TargetableType { get; }

        // an optional description of the Option, used in displaying help or error information
        string? Description { get; }

        // the first key defined for an option, sorted alphabetically (Options can define multiple keys
        // but they must be unique within the scope of all Options)
        string FirstKey { get; }

        // the keys (i.e, the 'x' in '-x') defined for an Option
        List<string> Keys { get; }

        // the optional default value assigned to the Option
        object? DefaultValue { get; }

        // the type of the Option, currently either Mappable or Null
        OptionType OptionType { get; }

        // flag indicating whether or not the option must be specified on the command line
        bool IsRequired { get; }

        // the minimum number of parameters to a command line option
        int MinParameters { get; }

        // the maximum number of parameters to a command line option
        int MaxParameters { get; }

        // the validator for the Option
        IOptionValidator Validator { get; }

        // the method called to validate the specified value within the expectations
        // defined for the Option
        bool Validate( IBindingTarget bindingTarget, string key, object value );

        // the method called to convert the parsing results for a particular command
        // line key to a option value. Return values other than MappingResults.Success
        // indicate one or more problems were encountered in the conversion and validation
        // process
        MappingResults Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            ITargetableType targetType,
            out object? result );
    }
}