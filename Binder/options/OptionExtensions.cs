using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public static class OptionExtensions
    {
        public static string ToTextPath( this List<string> contextKeys ) =>
            contextKeys.Aggregate(
                new StringBuilder(),
                ( sb, t ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( ":" );

                    sb.Append( t );

                    return sb;
                },
                sb => sb.ToString() );

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