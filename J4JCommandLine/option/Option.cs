using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // the abstract base class of Option and NullOption.
    public abstract class Option
    {
        private object? _defaultValue;

        protected Option(
            OptionType optionType,
            ITargetableType targetableType,
            OptionCollection options,
            CommandLineLogger logger
        )
        {
            OptionType = optionType;
            TargetableType = targetableType;
            Options = options;
            Logger = logger;
        }

        // the collection of Options used by the parsing activity
        public OptionCollection Options { get; }
        protected CommandLineLogger Logger { get; }

        // Information about the targetability of the type of the TargetProperty to
        // which the option is bound. Options can be bound to un-targetable properties.
        public ITargetableType TargetableType { get; }

        // an optional description of the Option, used in displaying help or error information
        public string Description { get; internal set; } = string.Empty;
        
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

        // flag indicating whether or not an option requires a parameter and whether or not
        // multiple parameters are allowed. Switches do not have parameters. Collections
        // require multiple parameters to be allowed.
        public OptionStyle OptionStyle { get; internal set; }

        // the validator for the Option
        public IOptionValidator? Validator { get; internal set; }
        
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
        public bool Validate( object value )
        {
            if( Validator == null )
                return true;

            if( value.GetType() == Validator.SupportedType )
                return Validator?.Validate( this, value, Logger ) ?? true;

            Logger.LogError(
                ProcessingPhase.Parsing,
                $"Object to be validated is a {value.GetType()} but should be a {Validator.SupportedType}, rejecting",
                option : this );

            return false;
        }

        // the method called to convert the parsing results for a particular command
        // line key to a option value. Return values other than MappingResult.Success
        // indicate one or more problems were encountered in the conversion and validation
        // process
        public abstract object? Convert( IAllocation allocation, ITargetableType targetType );

        // validates whether or not a valid number of parameters are included in the specified
        // IAllocation
        protected bool ValidParameterCount( IAllocation allocation )
        {
            // The UnkeyedOption allows for any number of parameters
            if( OptionType == OptionType.Unkeyed )
                return true;

            switch( OptionStyle )
            {
                case OptionStyle.Switch:
                    if (allocation.NumParameters > 0)
                        allocation.MoveExcessParameters(0);

                    break;

                case OptionStyle.SingleValued:
                    switch( allocation.NumParameters )
                    {
                        case 0:
                            Logger.LogError( 
                                ProcessingPhase.Parsing, 
                                $"Expected one parameter, got none",
                                option : this );

                            return false;

                        case 1:
                            // no op; desired situation
                            break;

                        default:
                            allocation.MoveExcessParameters(1);
                            break;
                    }

                    break;

                case OptionStyle.Collection:
                    // any number of a parameters is okay
                    break;

                default:
                    throw new NotSupportedException( $"Unsupported {nameof(OptionStyle)} '{OptionStyle}'" );
            }

            return true;
        }
    }
}