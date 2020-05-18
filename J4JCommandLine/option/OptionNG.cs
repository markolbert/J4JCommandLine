using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OptionNG : IOption
    {
        private readonly List<string> _keys = new List<string>();
        private readonly IOptionCollection _options;
        private readonly CommandLineErrors _errors;
        private readonly ITextConverter _converter;

        private IOptionValidator? _validator;

        public OptionNG( 
            IOptionCollection options,
            ITextConverter converter,
            CommandLineErrors errors,
            IJ4JLogger? logger = null 
        )
        {
            _options = options;
            _converter = converter;
            _errors = errors;
            Logger = logger;

            Logger?.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public ReadOnlyCollection<string> Keys => _keys.AsReadOnly();
        public object DefaultValue { get; private set; }

        public IOption AddKey( string key )
        {
            if( _options.HasKey( key ) )
                Logger?.Warning<string>( "Key '{key}' already in use", key );
            else _keys.Add( key );

            return this;
        }

        public IOption AddKeys(IEnumerable<string> keys)
        {
            foreach( var key in keys )
            {
                if (_options.HasKey(key))
                    Logger?.Warning<string>("Key '{key}' already in use", key);
                else _keys.Add(key);
            }

            return this;
        }

        public IOption SetDefaultValue( object defaultValue )
        {
            if( defaultValue.GetType() != _converter.SupportedType )
            {
                Logger?.Error<Type, Type>( 
                    "Default value is a {0} but should be a {1}", 
                    defaultValue.GetType(),
                    _converter.SupportedType );

                return this;
            }

            DefaultValue = defaultValue;

            Logger?.Verbose<string>( "Set default value to '{0}'", defaultValue?.ToString() ?? "**value**" );

            return this;
        }

        public IOption SetValidator( IOptionValidator validator )
        {
            if( validator.SupportedType != _converter.SupportedType )
            {
                Logger?.Error<Type, Type>(
                    "Validator works with a {0} but should validate a {1}",
                    validator.SupportedType,
                    _converter.SupportedType);

                return this;
            }

            _validator = validator;

            Logger?.Verbose( "Set validator" );

            return this;
        }

        public TextConversionResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out object result )
        {
            if (parseResult.NumParameters != 1)
            {
                result = DefaultValue;

                bindingTarget.AddError(parseResult.Key, $"Incorrect number of parameters. Expected 1, got {parseResult.NumParameters}");
                return TextConversionResult.FailedConversion;
            }

            if( _converter.Convert( parseResult.Parameters[ 0 ], out var innerResult ) )
            {
                result = innerResult;
                return TextConversionResult.Okay;
            }

            result = DefaultValue;

            bindingTarget.AddError(
                parseResult.Key,
                $"Couldn't convert '{parseResult.Parameters[0]}' to {_converter.SupportedType}");

            return TextConversionResult.FailedConversion;
        }

        public TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out List<object> result )
        {
            result = new List<object>();

            var allOkay = true;

            foreach( var parameter in parseResult.Parameters )
            {
                if( _converter.Convert( parameter, out var innerResult ) )
                    result.Add( innerResult );
                else
                {
                    bindingTarget.AddError(
                        parseResult.Key,
                        $"Couldn't convert '{parameter}' to {_converter.SupportedType}" );

                    allOkay = false;
                }
            }

            return allOkay ? TextConversionResult.Okay : TextConversionResult.FailedConversion;
        }

        public bool Validate( IBindingTarget bindingTarget, string key, object value )
        {
            if( _validator == null )
                return true;

            if( value.GetType() == _validator.SupportedType )
                return _validator?.Validate(bindingTarget, key, value) ?? true;

            Logger?.Error<Type, Type>( 
                "Object to be validated is a {0} but should be a {1}, rejecting", 
                value.GetType(),
                _validator.SupportedType );

            return false;
        }

        //bool IOption.Validate( IBindingTarget bindingTarget, string key, object value )
        //{
        //    if( value is TOption castValue )
        //        return Validate( bindingTarget, key, castValue );

        //    return true;
        //}

        //TextConversionResult IOption.Convert( 
        //    IBindingTarget bindingTarget, 
        //    IParseResult parseResult, 
        //    out object result )
        //{
        //    var retVal = Convert( bindingTarget, parseResult, out TOption cast );

        //    switch ( retVal )
        //    {
        //        case TextConversionResult.Okay:
        //            // this should never happen...
        //            if( cast == null )
        //            {
        //                result = new object();

        //                var elements = parseResult.NumParameters > 0
        //                    ? string.Join( ", ", parseResult.Parameters )
        //                    : "**null list**";

        //                var error = $"'{string.Join(", ", elements)}' got converted to a null object";
        //                _errors.AddError( bindingTarget, "", error );

        //                return TextConversionResult.ResultIsNull;
        //            }

        //            result = cast;

        //            return TextConversionResult.Okay;

        //        default:
        //            result = new object();
        //            return retVal;
        //    }
        //}

        //TextConversionResult IOption.ConvertList(
        //    IBindingTarget bindingTarget,
        //    IParseResult parseResult,
        //    out List<object> result)
        //{
        //    var retVal = ConvertList(bindingTarget, parseResult, out List<TOption> castResult);

        //    switch (retVal)
        //    {
        //        case TextConversionResult.Okay:
        //            // this should never happen...
        //            if (castResult == null)
        //            {
        //                result = new List<object>();

        //                var elements = parseResult.NumParameters > 0
        //                    ? string.Join(", ", parseResult.Parameters)
        //                    : "**null list**";

        //                var error = $"'{string.Join(", ", elements)}' got converted to a null object";
        //                _errors.AddError(bindingTarget, "", error);

        //                return TextConversionResult.ResultIsNull;
        //            }

        //            result = castResult.Cast<object>().ToList();

        //            return TextConversionResult.Okay;

        //        default:
        //            result = new List<object>();
        //            return retVal;
        //    }
        //}
    }
}