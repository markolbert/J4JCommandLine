using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Option<TOption> : IOption<TOption>
    {
        private readonly List<string> _keys = new List<string>();
        private readonly IOptionCollection _options;
        private readonly CommandLineErrors _errors;
        private readonly ITextConverter<TOption> _converter;

        private IOptionValidator<TOption>? _validator;

        public Option( 
            IOptionCollection options,
            ITextConverter<TOption> converter,
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

        public IOption<TOption> AddKey( string key )
        {
            if( _options.HasKey( key ) )
                Logger?.Warning<string>( "Key '{key}' already in use", key );
            else _keys.Add( key );

            return this;
        }

        public IOption<TOption> AddKeys(IEnumerable<string> keys)
        {
            foreach( var key in keys )
            {
                if (_options.HasKey(key))
                    Logger?.Warning<string>("Key '{key}' already in use", key);
                else _keys.Add(key);
            }

            return this;
        }

        public IOption<TOption> SetDefaultValue( TOption defaultValue )
        {
            DefaultValue = defaultValue;

            Logger?.Verbose<string>( "Set default value to '{0}'", defaultValue?.ToString() ?? "**value**" );

            return this;
        }

        public IOption<TOption> SetValidator( IOptionValidator<TOption> validator )
        {
            _validator = validator;

            Logger?.Verbose( "Set validator" );

            return this;
        }

        public TextConversionResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out TOption result )
        {
            if (parseResult.NumParameters != 1)
            {
                result = (TOption) DefaultValue;

                bindingTarget.AddError(parseResult.Key, $"Incorrect number of parameters. Expected 1, got {parseResult.NumParameters}");
                return TextConversionResult.FailedConversion;
            }

            if( _converter.Convert( parseResult.Parameters[ 0 ], out var innerResult ) )
            {
                result = innerResult;
                return TextConversionResult.Okay;
            }

            result = (TOption) DefaultValue;

            bindingTarget.AddError(
                parseResult.Key,
                $"Couldn't convert '{parseResult.Parameters[0]}' to {typeof(TOption)}");

            return TextConversionResult.FailedConversion;
        }

        public TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out List<TOption> result )
        {
            result = new List<TOption>();

            var allOkay = true;

            foreach( var parameter in parseResult.Parameters )
            {
                if( _converter.Convert( parameter, out var innerResult ) )
                    result.Add( innerResult );
                else
                {
                    bindingTarget.AddError(
                        parseResult.Key,
                        $"Couldn't convert '{parameter}' to {typeof(TOption)}" );

                    allOkay = false;
                }
            }

            return allOkay ? TextConversionResult.Okay : TextConversionResult.FailedConversion;
        }

        public bool Validate( IBindingTarget bindingTarget, string key, TOption value )
            => _validator?.Validate( bindingTarget, key, value ) ?? true;

        bool IOption.Validate( IBindingTarget bindingTarget, string key, object value )
        {
            if( value is TOption castValue )
                return Validate( bindingTarget, key, castValue );

            return true;
        }

        TextConversionResult IOption.Convert( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            out object result )
        {
            var retVal = Convert( bindingTarget, parseResult, out TOption cast );

            switch ( retVal )
            {
                case TextConversionResult.Okay:
                    // this should never happen...
                    if( cast == null )
                    {
                        result = new object();

                        var elements = parseResult.NumParameters > 0
                            ? string.Join( ", ", parseResult.Parameters )
                            : "**null list**";

                        var error = $"'{string.Join(", ", elements)}' got converted to a null object";
                        _errors.AddError( bindingTarget, "", error );

                        return TextConversionResult.ResultIsNull;
                    }

                    result = cast;

                    return TextConversionResult.Okay;

                default:
                    result = new object();
                    return retVal;
            }
        }

        TextConversionResult IOption.ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out List<object> result)
        {
            var retVal = ConvertList(bindingTarget, parseResult, out List<TOption> castResult);

            switch (retVal)
            {
                case TextConversionResult.Okay:
                    // this should never happen...
                    if (castResult == null)
                    {
                        result = new List<object>();

                        var elements = parseResult.NumParameters > 0
                            ? string.Join(", ", parseResult.Parameters)
                            : "**null list**";

                        var error = $"'{string.Join(", ", elements)}' got converted to a null object";
                        _errors.AddError(bindingTarget, "", error);

                        return TextConversionResult.ResultIsNull;
                    }

                    result = castResult.Cast<object>().ToList();

                    return TextConversionResult.Okay;

                default:
                    result = new List<object>();
                    return retVal;
            }
        }
    }
}