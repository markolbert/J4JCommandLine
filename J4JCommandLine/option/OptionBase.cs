using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
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
        protected internal IOptionCollection Options { get; }

        public ITargetableType TargetableType { get; }
        public string Description { get; internal set; }
        public List<string> Keys { get; } = new List<string>();
        public string FirstKey => Keys.Count == 0 ? string.Empty : Keys.OrderBy( k => k ).First();
        public OptionType OptionType { get; }
        public bool IsRequired { get; internal set; }
        public int MinParameters { get; internal set; }
        public int MaxParameters { get; internal set; } = int.MaxValue;
        public IOptionValidator Validator { get; internal set; }

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

        public abstract MappingResults Convert( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            ITargetableType targetType,
            out object? result );

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