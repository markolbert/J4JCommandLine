using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Option<TOption> : IOption<TOption>
    {
        private readonly List<string> _keys = new List<string>();
        private readonly IOptionCollection _options;

        public Option( 
            IOptionCollection options,
            IJ4JLogger? logger = null 
        )
        {
            _options = options;

            Logger = logger;

            Logger?.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger? Logger { get; }

        public ReadOnlyCollection<string> Keys => _keys.AsReadOnly();

        public Func<TOption>? GetDefaultValue { get; protected set; }

        public Func<TOption, bool>? Validator { get; protected set; }

        public IOption<TOption> AddKey( string key )
        {
            if( _options.HasKey( key ) )
                Logger?.Warning<string>( "Key '{key}' already in use", key );
            else _keys.Add( key );

            return this;
        }

        public IOption<TOption> SetDefaultValue( TOption defaultValue )
        {
            GetDefaultValue = () => defaultValue;

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

        bool IOption.IsValid( object toCheck )
        {
            if( toCheck is TOption cast )
                return IsValid( cast );

            Logger?.Warning<string, Type>( "{0} is not an instance of {1}", nameof(toCheck), typeof(TOption) );

            return false;
        }
    }
}