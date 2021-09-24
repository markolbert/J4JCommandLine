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

        private readonly ITextConverters _converters;
        private readonly IJ4JLogger? _logger;

        public BindabilityValidator(
            ITextConverters converters,
            IJ4JLogger? logger = null )
        {
            _converters = converters;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        protected bool IsBindable { get; private set; }
        protected string? PropertyPath { get; private set; }
        protected bool IsOuterMostLeaf { get; private set; }

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

        private bool CheckBasicBindability( Type toCheck )
        {
            if( toCheck.GetBindableInfo().BindableType != BindableType.Unsupported )
                return true;

            _logger?.Error( "{0} is neither a simple type, nor an array/list of simple types", toCheck );

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

                    _logger?.Error<string>( "Property '{0}' is indexed", propInfo.Name );
                    return false;
                }

                _logger?.Error<GetterSetter, string>( "The {0} for property '{1}' is not public",
                    getterSetter,
                    propInfo.Name );

                return false;
            }


            _logger?.Error<string, GetterSetter>( "Property '{0}' does not have a {1} method",
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

            if( !IsOuterMostLeaf || _converters.CanConvert( toCheck ) ) 
                return true;

            _logger?.Error( "No converter for text values exists for the property type '{0}'", toCheck );
        
            return false;
        }

        private bool CheckGenericType( Type toCheck )
        {
            if( !toCheck.IsGenericType )
                return false;

            if( toCheck.GenericTypeArguments.Length != 1 )
            {
                _logger?.Error( "Generic type '{0}' has more than one generic Type argument", toCheck );
                return false;
            }

            var genType = toCheck.GetGenericArguments()[ 0 ];

            if( !CheckNonGenericType( genType ) )
                return false;

            if( typeof(List<>).MakeGenericType( genType ).IsAssignableFrom( toCheck ) )
                return true;

            _logger?.Error( "Generic type '{0}' is not a List<> type", genType );

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

                _logger?.Error( "Array element Type '{0}' is undefined or not a valid non-generic type" );

                return false;
            }

            if( toCheck.IsValueType || typeof(string).IsAssignableFrom( toCheck ) )
                return true;

            if( !IsOuterMostLeaf
                || toCheck.GetConstructors().Any( c => !c.GetParameters().Any() ) )
                return true;

            _logger?.Error(
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

            _logger?.Error("Type '{0}' does not have any public parameterless constructors", toCheck);

            return false;
        }
    }
}