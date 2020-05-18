using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.CommandLine
{
    public class OptionBase : IOption
    {
        protected OptionBase(
            Type supportedType,
            IOptionCollection options,
            IJ4JLogger? logger 
            )
        {
            SupportedType = supportedType;
            Options = options;
            Logger = logger;

            Logger?.SetLoggedType( this.GetType() );
        }

        protected internal IJ4JLogger? Logger { get; }
        protected internal IOptionCollection Options { get; }

        public Type SupportedType { get; }
        public string Description { get; internal set; }
        public List<string> Keys {get;} = new List<string>();
        public object DefaultValue { get; internal set; }
        public bool IsRequired { get; internal set; }
        public int MinParameters { get; internal set; }
        public int MaxParameters { get; internal set; } = Int32.MaxValue;
        public IOptionValidator Validator { get; internal set; }

        //public IOption AddKey(string key)
        //{
        //    if (Options.HasKey(key))
        //        Logger?.Warning<string>("Key '{key}' already in use", key);
        //    else InternalKeys.Add(key);

        //    return this;
        //}

        //public IOption AddKeys(IEnumerable<string> keys)
        //{
        //    foreach (var key in keys)
        //    {
        //        if (Options.HasKey(key))
        //            Logger?.Warning<string>("Key '{key}' already in use", key);
        //        else InternalKeys.Add(key);
        //    }

        //    return this;
        //}

        //public IOption SetDefaultValue(object defaultValue)
        //{
        //    if (defaultValue.GetType() != SupportedType)
        //    {
        //        Logger?.Error<Type, Type>(
        //            "Default value is a {0} but should be a {1}",
        //            defaultValue.GetType(),
        //            SupportedType);

        //        return this;
        //    }

        //    DefaultValue = defaultValue;

        //    Logger?.Verbose<string>("Set default value to '{0}'", defaultValue?.ToString() ?? "**value**");

        //    return this;
        //}

        //public IOption Required()
        //{
        //    IsRequired = true;
        //    return this;
        //}

        //public IOption Optional()
        //{
        //    IsRequired = false;
        //    return this;
        //}

        //public IOption ArgumentCount( int minimum, int maximum = Int32.MaxValue )
        //{
        //    minimum = minimum < 0 ? 0 : minimum;
        //    maximum = maximum < 0 ? Int32.MaxValue : maximum;

        //    if( minimum > maximum )
        //    {
        //        var temp = maximum;

        //        maximum = minimum;
        //        minimum = temp;
        //    }

        //    MinParameters = minimum;
        //    MaxParameters = maximum;

        //    return this;
        //}

        //public IOption SetDescription( string description )
        //{
        //    Description = description;
        //    return this;
        //}

        //public IOption SetValidator(IOptionValidator validator)
        //{
        //    if (validator.SupportedType != SupportedType)
        //    {
        //        Logger?.Error<Type, Type>(
        //            "Validator works with a {0} but should validate a {1}",
        //            validator.SupportedType,
        //            SupportedType);

        //        return this;
        //    }

        //    Validator = validator;

        //    Logger?.Verbose("Set validator");

        //    return this;
        //}

        public bool Validate( IBindingTarget bindingTarget, string key, object value )
        {
            if( Validator == null )
                return true;

            if( value.GetType() == Validator.SupportedType )
                return Validator?.Validate( bindingTarget, key, value ) ?? true;

            Logger?.Error<Type, Type>(
                "Object to be validated is a {0} but should be a {1}, rejecting",
                value.GetType(),
                Validator.SupportedType );

            return false;
        }

        public virtual TextConversionResult Convert( IBindingTarget bindingTarget, IParseResult parseResult, out object result )
        {
            throw new NotImplementedException();
        }

        public virtual IList CreateEmptyList()
        {
            throw new NotImplementedException();
        }

        public virtual TextConversionResult ConvertList( IBindingTarget bindingTarget, IParseResult parseResult, out IList result )
        {
            throw new NotImplementedException();
        }
    }
}
