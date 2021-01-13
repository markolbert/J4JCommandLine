using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Configuration.CommandLine
{
    public class DefaultPropertyValidator : PropertyValidatorBase
    {
        public DefaultPropertyValidator( 
            IConverters converters,
            IJ4JLogger? logger 
            ) 
            : base( converters, logger )
        {
        }

        public override bool IsPropertyBindable( Stack<PropertyInfo> propertyStack )
        {
            base.IsPropertyBindable( propertyStack );

            var topProp = propertyStack.Peek();

            CheckGetter( topProp );
            CheckType( topProp.PropertyType );

            if( !IsOuterMostLeaf ) 
                return IsBindable;
            
            CheckSetter(topProp);
            CheckConstructor(topProp.PropertyType);

            return IsBindable;
        }

        private void CheckGetter(PropertyInfo propInfo )
        {
            if( propInfo.GetMethod == null )
                LogError( "Property does not have a get method" );
            else
            {
                if( !propInfo.GetMethod.IsPublic )
                    LogError( "Property's getter is not public" );

                if( propInfo.GetMethod.GetParameters().Any() )
                    LogError( "Property's getter is indexed" );
            }
        }

        private void CheckSetter(PropertyInfo propInfo)
        {
            if (propInfo.SetMethod == null)
                LogError("Property does not have a get method");
            else
            {
                if (!propInfo.SetMethod.IsPublic)
                    LogError("Property's getter is not public");

                if (propInfo.SetMethod.GetParameters().Length > 1)
                    LogError("Property's setter is indexed");
            }
        }

        private void CheckType( Type toCheck )
        {
            if( toCheck.IsGenericType )
                CheckGenericType( toCheck );
            else CheckNonGenericType( toCheck );

            if( IsOuterMostLeaf && !CanConvert( toCheck ) )
                LogError( "No converter for text values exists for the property type" );
        }

        private void CheckGenericType(Type toCheck)
        {
            if (!toCheck.IsGenericType)
                return;

            if( toCheck.GenericTypeArguments.Length != 1 )
                LogError( "Generic type has more than one generic Type argument" );
            else
            {
                var genType = toCheck.GetGenericArguments()[ 0 ];
                CheckNonGenericType( genType, "from generic type check" );

                if (!typeof(List<>).MakeGenericType(genType).IsAssignableFrom(toCheck))
                    LogError("Generic type is not a List<> type");
            }
        }

        private void CheckNonGenericType(Type toCheck, string? hint = null )
        {
            if (toCheck.IsGenericType)
                LogError("Type must be non-generic", hint);
            else
            {
                if (toCheck.IsArray)
                    CheckNonGenericType( toCheck.GetElementType()!, "from array type check" );
                else
                {
                    if (toCheck.IsValueType || typeof(string).IsAssignableFrom(toCheck))
                        return;

                    if (!IsOuterMostLeaf
                        || toCheck.GetConstructors().Any(c => !c.GetParameters().Any()))
                        return;

                    LogError(
                        "Type is neither a ValueType nor a string type and does not have a public parameterless constructor");
                }
            }
        }

        private void CheckConstructor( Type toCheck )
        {
            // strings are reference types but act like value types from a construction point of view
            // value types don't have and don't need constructors
            if (toCheck.IsValueType || typeof(string).IsAssignableFrom(toCheck))
                return;

            if (toCheck.GetConstructors().All(c => c.GetParameters().Length != 0))
                LogError("Type does not have any public parameterless constructors");
        }
    }
}