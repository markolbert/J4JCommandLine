using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    // an abstract class implementing IOption. The base of Option and NullOption.
    public abstract class OptionBase : IOption
    {
        private object? _defaultValue;

        protected OptionBase(
            OptionType optionType,
            ITargetableType targetableType,
            IOptionCollection options,
            IJ4JLogger? logger
        )
        {
            OptionType = optionType;
            TargetableType = targetableType;
            Options = options;
            Logger = logger;

            Logger?.SetLoggedType( GetType() );
        }

        protected internal IJ4JLogger? Logger { get; }

        // the collection of Options used by the parsing activity
        protected internal IOptionCollection Options { get; }

        // Information about the targetability of the type of the TargetProperty to
        // which the option is bound. Options can be bound to un-targetable properties.
        public ITargetableType TargetableType { get; }

        // an optional description of the Option, used in displaying help or error information
        public string Description { get; internal set; }
        
        // the first key defined for an option, sorted alphabetically (Options can define multiple keys
        // but they must be unique within the scope of all Options)
        public List<string> Keys { get; } = new List<string>();

        // the first key defined for an option, sorted alphabetically (Options can define multiple keys
        // but they must be unique within the scope of all Options)
        public string FirstKey => Keys.Count == 0 ? string.Empty : Keys.OrderBy( k => k ).First();

        // the type of the Option, currently either Mappable or Null
        public OptionType OptionType { get; }

        // flag indicating whether or not the option must be specified on the command line
        public bool IsRequired { get; internal set; }

        // the minimum number of parameters to a command line option
        public int MinParameters { get; internal set; }

        // the maximum number of parameters to a command line option
        public int MaxParameters { get; internal set; } = int.MaxValue;

        // the validator for the Option
        public IOptionValidator Validator { get; internal set; }
        
        // the optional default value assigned to the Option
        public object? DefaultValue
        {
            get
            {
                if( _defaultValue == null && TargetableType.IsCreatable )
                    _defaultValue = TargetableType.GetDefaultValue();

                return _defaultValue;
            }

            internal set => _defaultValue = value;
        }


        // the method called to validate the specified value within the expectations
        // defined for the Option
        public bool Validate( IBindingTarget bindingTarget, string key, object value )
        {
            if( Validator == null )
                return true;

            if( value.GetType() == Validator.SupportedType )
                return Validator?.Validate( bindingTarget, key, value ) ?? true;

            Logger?.Error(
                "Object to be validated is a {0} but should be a {1}, rejecting",
                value.GetType(),
                Validator.SupportedType );

            return false;
        }


        // the method called to convert the parsing results for a particular command
        // line key to a option value. Return values other than MappingResults.Success
        // indicate one or more problems were encountered in the conversion and validation
        // process
        public abstract MappingResults Convert( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            ITargetableType targetType,
            out object? result );

        // validates whether or not a valid number of parameters are included in the specified
        // IParseResult
        protected virtual bool ValidParameterCount(IParseResult parseResult, out MappingResults result)
        {
            if (parseResult.NumParameters < MinParameters)
            {
                Logger?.Error<int, int>("Expected {0} parameters, got {1}", MinParameters, parseResult.NumParameters);
                result = MappingResults.TooFewParameters;

                return false;
            }

            if (parseResult.NumParameters > MaxParameters)
            {
                Logger?.Error<int, int>("Expected {0} parameters, got {1}", MinParameters, parseResult.NumParameters);
                result = MappingResults.TooManyParameters;

                return false;
            }

            result = MappingResults.Success;

            return true;
        }
    }
}