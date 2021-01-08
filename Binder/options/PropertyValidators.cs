using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public static class PropertyValidators
    {
        public static ValidationEntry<PropertyInfo> HasSupportedGetter( this ValidationEntry<PropertyInfo> entry )
        {
            if (entry.Value.GetMethod == null)
                entry.AddError( "Property does not have a get method" );
            else
            {
                if (!entry.Value.GetMethod.IsPublic )
                    entry.AddError("Property's getter is not public");

                if (entry.Value.GetMethod.GetParameters().Any())
                    entry.AddError( "Property's getter is indexed" );
            }

            return entry;
        }

        public static ValidationEntry<PropertyInfo> HasSupportedSetter( this ValidationEntry<PropertyInfo> entry )
        {
            if( entry.Value.SetMethod == null )
                entry.AddError( "Property does not have a set method" );
            else
            {
                if( !entry.Value.SetMethod.IsPublic )
                    entry.AddError( "Property's setter is not public" );

                if( entry.Value.SetMethod.GetParameters().Length > 1 )
                    entry.AddError( "Property's setter is indexed" );
            }

            return entry;
        }

        public static ValidationEntry<Type> IsSupportedType( this ValidationEntry<Type> entry )
        {
            if( entry.Value.IsGenericType )
                entry.IsSupportedGenericType();
            else entry.IsSupportedNonGenericType();

            return entry;
        }

        public static ValidationEntry<Type> IsSupportedGenericType( this ValidationEntry<Type> entry )
        {
            if( !entry.Value.IsGenericType )
                return entry;

            if (entry.Value.GenericTypeArguments.Length != 1)
                entry.AddError("Generic type has more than one generic Type argument" );
            else
            {
                var typeParamEntry =
                    entry.CreateChild<Type>( entry.Value.GetGenericArguments()[ 0 ], "from generic type check" );

                typeParamEntry.IsSupportedNonGenericType();

                if (!typeof(List<>).MakeGenericType(entry.Value.GenericTypeArguments[0]).IsAssignableFrom(entry.Value))
                    entry.AddError("Generic type is not a List<> type");
            }

            return entry;
        }

        public static ValidationEntry<Type> IsSupportedNonGenericType( this ValidationEntry<Type> entry )
        {
            if( entry.Value.IsGenericType )
                entry.AddError( "Type must be non-generic" );
            else
            {
                if( entry.Value.IsArray )
                {
                    var arrayTypeEntry =
                        entry.CreateChild<Type>( entry.Value.GetElementType()!, "from array type check" );

                    arrayTypeEntry.IsSupportedNonGenericType();
                }
                else
                {
                    if( entry.Value.IsValueType
                        || typeof(string).IsAssignableFrom( entry.Value )
                        || entry.Value.GetConstructors().Any( c => !c.GetParameters().Any() ) )
                        return entry;

                    entry.AddError(
                        "Type is neither a ValueType nor a string type and does not have a public parameterless constructor" );
                }
            }

            return entry;
        }

        public static ValidationEntry<Type> HasRequiredConstructor( this ValidationEntry<Type> entry )
        {
            // strings are reference types but act like value types from a construction point of view
            // value types don't have and don't need constructors
            if (entry.Value.IsValueType || typeof(string).IsAssignableFrom(entry.Value))
                return entry;

            if ( entry.Value.GetConstructors().All( c => c.GetParameters().Length != 0 ) )
                entry.AddError( "Type does not have any public parameterless constructors" );

            return entry;
        }

        public static OptionStyle GetOptionStyle( this PropertyInfo propInfo )
        {
            if( propInfo.PropertyType.IsEnum )
                return propInfo.PropertyType.GetCustomAttribute<FlagsAttribute>() != null
                    ? OptionStyle.ConcatenatedSingleValue
                    : OptionStyle.SingleValued;

            // we assume any generic type is a collection-style option because
            // the only generic types we support are List<>s
            if( propInfo.PropertyType.IsGenericType )
                return OptionStyle.Collection;

            return propInfo.PropertyType.IsArray
                ? OptionStyle.Collection
                : typeof(bool).IsAssignableFrom(propInfo.PropertyType)
                    ? OptionStyle.Switch
                    : OptionStyle.SingleValued;
        }
    }
}
