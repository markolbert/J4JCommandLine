using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Option<TOption> : IOption<TOption>
    {
        private readonly List<string> _keys = new List<string>();
        private readonly IOptionCollection _options;

        public Option( 
            TOption initialDefault,
            IOptionCollection options,
            IJ4JLogger? logger = null 
        )
        {
            DefaultValue = initialDefault;
            _options = options;
            Logger = logger;

            Logger?.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public ReadOnlyCollection<string> Keys => _keys.AsReadOnly();

        public TOption DefaultValue { get; private set; }

        public Func<TOption, bool>? Validator { get; protected set; }

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

        public IOption<TOption> SetValidator( Func<TOption, bool> validator )
        {
            Validator = validator;

            Logger?.Verbose( "Set validator" );

            return this;
        }

        public bool IsValid( TOption toCheck )
        {
            if( Validator == null )
            {
                Logger?.Verbose<string>("No {0} defined, assuming value is valid", nameof(Validator));
                return true;
            }

            if( Validator( toCheck ) )
            {
                Logger?.Verbose<string>( "Value {0} is valid", toCheck?.ToString() ?? "**value**" );
                return true;
            }

            Logger?.Information<string>("Value {0} is invalid", toCheck?.ToString() ?? "**value**");

            return false;
        }

        public virtual TextConversionResult Convert( List<string>? textElements, out TOption result, out string? error )
        {
            result = DefaultValue;
            error = null;

            return TextConversionResult.Okay;
        }

        bool IOption.IsValid( object toCheck )
        {
            if( toCheck is TOption cast )
                return IsValid( cast );

            Logger?.Warning<string, Type>( "{0} is not an instance of {1}", nameof(toCheck), typeof(TOption) );

            return false;
        }

        TextConversionResult IOption.Convert( List<string>? textElements, out object result, out string? error )
        {
            var retVal = Convert( textElements, out TOption cast, out string? innerError );

            switch ( retVal )
            {
                case TextConversionResult.Okay:
                    // this should never happen...
                    if( cast == null )
                    {
                        result = new object();

                        var elements = textElements == null ? "**null list**" : string.Join( ", ", textElements );
                        error = $"'{string.Join(", ", elements)}' got converted to a null object";

                        return TextConversionResult.ResultIsNull;
                    }

                    result = cast;
                    error = null;

                    return TextConversionResult.Okay;

                default:
                    result = new object();
                    error = innerError;

                    return retVal;
            }
        }
    }
}