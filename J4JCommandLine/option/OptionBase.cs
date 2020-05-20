using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OptionBase : IOption
    {
        protected OptionBase(
            OptionType optionType,
            Type supportedType,
            IOptionCollection options,
            IJ4JLogger? logger
        )
        {
            OptionType = optionType;
            SupportedType = supportedType;
            Options = options;
            Logger = logger;

            Logger?.SetLoggedType( GetType() );
        }

        protected internal IJ4JLogger? Logger { get; }
        protected internal IOptionCollection Options { get; }

        public Type SupportedType { get; }
        public string Description { get; internal set; }
        public List<string> Keys { get; } = new List<string>();
        public string FirstKey => Keys.Count == 0 ? string.Empty : Keys.OrderBy( k => k ).First();
        public object DefaultValue { get; internal set; }
        public OptionType OptionType { get; }
        public bool IsRequired { get; internal set; }
        public int MinParameters { get; internal set; }
        public int MaxParameters { get; internal set; } = int.MaxValue;
        public IOptionValidator Validator { get; internal set; }

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

        public virtual TextConversionResult Convert( IBindingTarget bindingTarget, IParseResult parseResult,
            out object result )
        {
            throw new NotImplementedException();
        }

        public virtual IList CreateEmptyList()
        {
            throw new NotImplementedException();
        }

        public virtual Array CreateEmptyArray( int capacity )
        {
            throw new NotImplementedException();
        }

        public virtual TextConversionResult ConvertList( IBindingTarget bindingTarget, IParseResult parseResult,
            out IList result )
        {
            throw new NotImplementedException();
        }
    }
}