using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public static class OptionExtensions
    {
        public static T AddKey<T>( this T option, string key )
            where T : OptionBase
        {
            if( option.Options.HasKey( key ) )
                option.Logger?.Warning<string>( "Key '{key}' already in use", key );
            else option.Keys.Add( key );

            return option;
        }

        public static T AddKeys<T>( this T option, IEnumerable<string> keys )
            where T : OptionBase
        {
            foreach( var key in keys )
                if( option.Options.HasKey( key ) )
                    option.Logger?.Warning<string>( "Key '{key}' already in use", key );
                else option.Keys.Add( key );

            return option;
        }

        public static T SetDefaultValue<T>( this T option, object defaultValue )
            where T : OptionBase
        {
            if( defaultValue.GetType() != option.SupportedType )
            {
                option.Logger?.Error(
                    "Default value is a {0} but should be a {1}",
                    defaultValue.GetType(),
                    option.SupportedType );

                return option;
            }

            option.DefaultValue = defaultValue;

            option.Logger?.Verbose<string>( "Set default value to '{0}'", defaultValue?.ToString() ?? "**value**" );

            return option;
        }

        public static T Required<T>( this T option )
            where T : OptionBase
        {
            option.IsRequired = true;

            return option;
        }

        public static T Optional<T>( this T option )
            where T : OptionBase
        {
            option.IsRequired = false;

            return option;
        }

        public static T ArgumentCount<T>( this T option, int minimum, int maximum = int.MaxValue )
            where T : OptionBase
        {
            minimum = minimum < 0 ? 0 : minimum;
            maximum = maximum < 0 ? int.MaxValue : maximum;

            if( minimum > maximum )
            {
                var temp = maximum;

                maximum = minimum;
                minimum = temp;
            }

            option.MinParameters = minimum;
            option.MaxParameters = maximum;

            return option;
        }

        public static T SetDescription<T>( this T option, string description )
            where T : OptionBase
        {
            option.Description = description;

            return option;
        }

        public static T SetValidator<T>( this T option, IOptionValidator validator )
            where T : OptionBase
        {
            if( validator.SupportedType != option.SupportedType )
            {
                option.Logger?.Error(
                    "Validator works with a {0} but should validate a {1}",
                    validator.SupportedType,
                    option.SupportedType );

                return option;
            }

            option.Validator = validator;

            option.Logger?.Verbose( "Set validator" );

            return option;
        }
    }
}