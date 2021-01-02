using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine.Deprecated
{
    public static class OptionExtensions
    {
        // adds the specified keys to the collection of keys defined for the Option provided each one
        // is not already in use by another Option (skips duplicates)
        public static T AddKeys<T>(this T option, IEnumerable<string> keys)
            where T : Option
        {
            if (option.OptionType == OptionType.Null)
                return option;

            foreach (var key in keys)
            {
                if (!option.Options.HasKey(key))
                    option.Keys.Add(key);
            }

            return option;
        }

        // Checks to see if the specified Option is an Option (and not a NullOption, which has no
        // need of a default value) and that the proposed default value matches the type the Option is
        // working with and, if both conditions are met, sets Option's default value
        public static T SetDefaultValue<T>(this T option, object defaultValue)
            where T : Option
        {
            if (option.OptionType != OptionType.Keyed)
                return option;

            if (defaultValue.GetType() != option.TargetableType.SupportedType)
                return option;

            option.DefaultValue = defaultValue;

            return option;
        }

        // marks the specified Option as being required (i.e, must appear in the command line
        // arguments) provided it's an Option and not a NullOption
        public static T Required<T>( this T option )
            where T : Option
        {
            if( option.OptionType != OptionType.Keyed )
                return option;

            option.IsRequired = true;

            return option;
        }

        // marks the specified Option as being optional (which is the default) provided
        // it's an Option and not a NullOption
        public static T Optional<T>( this T option )
            where T : Option
        {
            if( option.OptionType != OptionType.Keyed )
                return option;

            option.IsRequired = false;

            return option;
        }

        // sets the optional description for an Option. Ignored if the specified Option object is
        // a NullOption.
        public static T SetDescription<T>( this T option, string description )
            where T : Option
        {
            if( option.OptionType == OptionType.Null )
                return option;

            option.Description = description;

            return option;
        }

        // sets the validator for an Option. Ignored if the specified Option object is
        // a NullOption.
        public static T SetValidator<T>( this T option, IOptionValidator validator )
            where T : Option
        {
            if( option.OptionType != OptionType.Keyed )
                return option;

            if( validator.SupportedType != option.TargetableType.SupportedType )
                return option;

            option.Validator = validator;

            return option;
        }

        // creates a list of all combinations of the specified key and the allowed key prefixes
        // (e.g., "-x, --x, /x" for 'x')
        public static List<string> ConjugateKey( this MasterTextCollection masterText, string key )
        {
            var retVal = new List<string>();

            if( string.IsNullOrEmpty( key ) )
                return retVal;

            retVal.AddRange(
                masterText[TextUsageType.Prefix].Aggregate(
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

        // creates a list of all combinations of an Option's keys and the allowed key prefixes
        // (e.g., "-x, --x, /x" for 'x')
        public static List<string> ConjugateKeys(this Option option, MasterTextCollection masterText)
        {
            var retVal = new List<string>();

            if (option.OptionType == OptionType.Null)
                return retVal;

            return option.Keys.Aggregate(
                retVal,
                (list, key) =>
                {
                    list.AddRange(
                        masterText[TextUsageType.Prefix].Aggregate(
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