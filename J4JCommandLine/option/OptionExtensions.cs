using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public static class OptionExtensions
    {
        public static T AddKey<T>( this T option, string key )
            where T : OptionBase
        {
            if( option.OptionType == OptionType.Null )
            {
                option.Logger?.Warning<string>( "Trying to add '{key}' to a NullOption's keys, ignoring", key );
                return option;
            }

            if( option.Options.HasKey( key ) )
                option.Logger?.Warning<string>( "Key '{key}' already in use", key );
            else option.Keys.Add( key );

            return option;
        }

        public static T AddKeys<T>( this T option, IEnumerable<string> keys )
            where T : OptionBase
        {
            if( option.OptionType == OptionType.Null )
            {
                option.Logger?.Warning<string>(
                    "Trying to add '{0}' to a NullOption's keys, ignoring",
                    string.Join( ",", keys ) );

                return option;
            }

            foreach( var key in keys )
                if( option.Options.HasKey( key ) )
                    option.Logger?.Warning<string>( "Key '{key}' already in use", key );
                else option.Keys.Add( key );

            return option;
        }

        public static T SetDefaultValue<T>( this T option, object defaultValue )
            where T : OptionBase
        {
            if( option.OptionType != OptionType.Mappable )
            {
                option.Logger?.Warning( "Trying to add set default value for a NullOption or HelpOption, ignoring" );

                return option;
            }

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
            if( option.OptionType != OptionType.Mappable )
            {
                option.Logger?.Warning( "Trying to require a NullOption or HelpOption, ignoring" );

                return option;
            }

            option.IsRequired = true;

            return option;
        }

        public static T Optional<T>( this T option )
            where T : OptionBase
        {
            if( option.OptionType != OptionType.Mappable )
            {
                option.Logger?.Warning( "Trying to make a NullOption or HelpOption optional, ignoring" );

                return option;
            }

            option.IsRequired = false;

            return option;
        }

        public static T ArgumentCount<T>( this T option, int minimum, int maximum = int.MaxValue )
            where T : OptionBase
        {
            if( option.OptionType != OptionType.Mappable )
            {
                option.Logger?.Warning( "Trying to set argument limits on a NullOption or HelpOption, ignoring" );

                return option;
            }

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
            if( option.OptionType == OptionType.Null )
            {
                option.Logger?.Warning( "Trying to set a description on a NullOption, ignoring" );

                return option;
            }

            option.Description = description;

            return option;
        }

        public static T SetValidator<T>( this T option, IOptionValidator validator )
            where T : OptionBase
        {
            if( option.OptionType != OptionType.Mappable )
            {
                option.Logger?.Warning( "Trying to set a validator on a NullOption or HelpOption, ignoring" );

                return option;
            }

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

        public static List<string> ConjugateKey( this IParsingConfiguration parseConfig, string key )
        {
            var retVal = new List<string>();

            if( string.IsNullOrEmpty( key ) )
                return retVal;

            retVal.AddRange(
                parseConfig.Prefixes.Aggregate(
                    new List<string>(),
                    ( innerList, delim ) =>
                    {
                        innerList.Add( $"{delim}{key}" );

                        return innerList;
                    }
                )
            );

            return retVal;
        }

        public static List<string> ConjugateKeys(this IOption option, IParsingConfiguration parseConfig)
        {
            var retVal = new List<string>();

            if (option.OptionType == OptionType.Null)
                return retVal;

            return option.Keys.Aggregate(
                retVal,
                (list, key) =>
                {
                    list.AddRange(
                        parseConfig.Prefixes.Aggregate(
                            new List<string>(),
                            (innerList, delim) =>
                            {
                                innerList.Add($"{delim}{key}");

                                return innerList;
                            }
                        )
                    );

                    return list;
                });
        }
    }
}