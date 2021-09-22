#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Configuration.CommandLine
{
    public class BindabilityValidator : IBindabilityValidator
    {
        private enum GetterSetter
        {
            Getter,
            Setter
        }

        public static List<ITextToValue> GetBuiltInConverters( IJ4JLogger? logger )
        {
            var retVal = new List<ITextToValue>();

            foreach( var convMethod in typeof(Convert)
                .GetMethods( BindingFlags.Static | BindingFlags.Public )
                .Where( m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 && !typeof(string).IsAssignableFrom( parameters[ 0 ].ParameterType );
                } ) )
            {
                var builtInType = typeof(BuiltInTextToValue<>).MakeGenericType( convMethod.ReturnType );

                retVal.Add( (ITextToValue)Activator.CreateInstance(
                        builtInType,
                        new object?[] { convMethod, logger } )!
                );
            }

            return retVal;
        }

        private readonly List<ITextToValue> _converters;

        public BindabilityValidator(
            IJ4JLogger? logger = null
        )
            : this( GetBuiltInConverters( logger ), logger )
        {
        }

        public BindabilityValidator(
            IEnumerable<ITextToValue> converters,
            IJ4JLogger? logger = null )
        {
            _converters = converters.ToList();

            Logger = logger;
            Logger?.SetLoggedType( GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected bool IsBindable { get; private set; }
        protected string? PropertyPath { get; private set; }
        protected bool IsOuterMostLeaf { get; private set; }

        public bool CanConvert( Type toCheck )
        {
            // we can convert any type for which we have a converter, plus lists and arrays of those types
            if( toCheck.IsArray )
            {
                var elementType = toCheck.GetElementType();
                return elementType != null && can_convert_simple( elementType );
            }

            if( toCheck.IsGenericType )
            {
                var genArgs = toCheck.GetGenericArguments();
                if( genArgs.Length != 1 )
                    return false;

                if( !can_convert_simple( genArgs[ 0 ] ) )
                    return false;

                return ( typeof(List<>).MakeGenericType( genArgs[ 0 ] )
                    .IsAssignableFrom( toCheck ) );
            }

            if( can_convert_simple( toCheck ) )
                return true;

            Logger?.Error( "No ITextToValue converter is defined for {0}", toCheck );

            return false;

            bool can_convert_simple( Type simpleType )
            {
                if( simpleType.IsArray || simpleType.IsGenericType )
                    return false;

                return simpleType.IsEnum 
                       || _converters.Any( x => x.CanConvert( simpleType ) );
            }
        }

        public bool Convert( Type targetType, IEnumerable<string> textValues, out object? result )
        {
            result = null;

            var converter = _converters.FirstOrDefault( x => x.CanConvert( targetType ) );
            
            if( converter != null ) 
                return converter.Convert( textValues, out result );

            if( targetType.IsEnum )
            {
                var enumConverterType = typeof(TextToEnum<>).MakeGenericType( targetType );
                converter = Activator.CreateInstance( enumConverterType, new object?[] { Logger } ) as ITextToValue;
                _converters.Add( converter! );

                return converter!.Convert( textValues, out result );
            }
            
            Logger?.Error( "Cannot convert text to '{0}'", targetType );
            
            return false;
        }

        public virtual bool IsPropertyBindable( Stack<PropertyInfo> propertyStack )
        {
            IsBindable = true;

            PropertyPath = propertyStack.Aggregate(
                new StringBuilder(),
                ( sb, pi ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( ":" );

                    sb.Append( pi.Name );

                    return sb;
                },
                sb => sb.ToString()
            );

            IsOuterMostLeaf = propertyStack.Count == 1;

            var topProp = propertyStack.Peek();

            IsBindable = CheckBasicBindability( propertyStack.Peek().PropertyType )
                         && CheckGetterSetter( topProp, GetterSetter.Getter )
                         && CheckType( topProp.PropertyType );

            if( !IsOuterMostLeaf || !IsBindable )
                return IsBindable;

            IsBindable &= CheckGetterSetter( topProp, GetterSetter.Setter )
                          && CheckConstructor( topProp.PropertyType );

            return IsBindable;
        }

        public bool IsPropertyBindable( Type toCheck )
        {
            PropertyPath = toCheck.Name;
            IsOuterMostLeaf = true;

            IsBindable = CheckBasicBindability(toCheck);

            return IsBindable;
        }

        private bool CheckBasicBindability( Type toCheck )
        {
            if( toCheck.GetBindableInfo().BindableType != BindableType.Unsupported )
                return true;

            Logger?.Error( "{0} is neither a simple type, nor an array/list of simple types", toCheck );

            return false;
        }

        private bool CheckGetterSetter( PropertyInfo propInfo, GetterSetter getterSetter )
        {
            var (toCheck, reqdParams) = getterSetter switch
            {
                GetterSetter.Getter => (propInfo.GetMethod, 0),
                _ => (propInfo.SetMethod, 1)
            };

            if( toCheck != null )
            {
                if( toCheck.IsPublic )
                {
                    if( toCheck.GetParameters().Length == reqdParams )
                        return true;

                    Logger?.Error<string>( "Property '{0}' is indexed", propInfo.Name );
                    return false;
                }

                Logger?.Error<GetterSetter, string>( "The {0} for property '{1}' is not public",
                    getterSetter,
                    propInfo.Name );

                return false;
            }


            Logger?.Error<string, GetterSetter>( "Property '{0}' does not have a {1} method",
                propInfo.Name,
                getterSetter );

            return false;
        }

        private bool CheckType( Type toCheck )
        {
            if( toCheck.IsEnum )
                return true;

            if( toCheck.IsGenericType )
            {
                if( !CheckGenericType( toCheck ) )
                    return false;
            }
            else
            {
                if( !CheckNonGenericType( toCheck ) )
                    return false;
            }

            if( !IsOuterMostLeaf || CanConvert( toCheck ) ) 
                return true;

            Logger?.Error( "No converter for text values exists for the property type '{0}'", toCheck );
        
            return false;
        }

        private bool CheckGenericType( Type toCheck )
        {
            if( !toCheck.IsGenericType )
                return false;

            if( toCheck.GenericTypeArguments.Length != 1 )
            {
                Logger?.Error( "Generic type '{0}' has more than one generic Type argument", toCheck );
                return false;
            }

            var genType = toCheck.GetGenericArguments()[ 0 ];

            if( !CheckNonGenericType( genType ) )
                return false;

            if( typeof(List<>).MakeGenericType( genType ).IsAssignableFrom( toCheck ) )
                return true;

            Logger?.Error( "Generic type '{0}' is not a List<> type", genType );

            return false;
        }

        private bool CheckNonGenericType( Type toCheck )
        {
            if( toCheck.IsGenericType )
                return false;

            if( toCheck.IsArray )
            {
                var elementType = toCheck.GetElementType();

                if( elementType != null && CheckNonGenericType( elementType ) )
                    return true;

                Logger?.Error( "Array element Type '{0}' is undefined or not a valid non-generic type" );

                return false;
            }

            if( toCheck.IsValueType || typeof(string).IsAssignableFrom( toCheck ) )
                return true;

            if( !IsOuterMostLeaf
                || toCheck.GetConstructors().Any( c => !c.GetParameters().Any() ) )
                return true;

            Logger?.Error(
                "'{0}' is neither a ValueType nor a string type and does not have a public parameterless constructor",
                toCheck );

            return false;
        }

        private bool CheckConstructor(Type toCheck)
        {
            // strings are reference types but act like value types from a construction point of view
            // value types don't have and don't need constructors
            if (toCheck.IsValueType || typeof(string).IsAssignableFrom(toCheck))
                return true;

            if( toCheck.GetConstructors().Any( c => c.GetParameters().Length == 0 ) )
                return true;

            Logger?.Error("Type '{0}' does not have any public parameterless constructors", toCheck);

            return false;
        }
    }
}