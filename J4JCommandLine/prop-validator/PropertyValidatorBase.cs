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

namespace J4JSoftware.Configuration.CommandLine
{
    public class PropertyValidatorBase : IPropertyValidator
    {
        private readonly IConverters _converters;

        protected PropertyValidatorBase(
            IConverters converters,
            IJ4JLogger? logger )
        {
            _converters = converters;
            Logger = logger;
        }

        protected IJ4JLogger? Logger { get; }
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

            CheckBasicBindability( propertyStack.Peek().PropertyType );

            return IsBindable;
        }

        public virtual bool IsPropertyBindable( Type propType )
        {
            IsBindable = true;
            PropertyPath = propType.Name;
            IsOuterMostLeaf = true;

            CheckBasicBindability( propType );

            return IsBindable;
        }

        protected bool CanConvert( Type toCheck )
        {
            return _converters.CanConvert( toCheck );
        }

        protected void LogError( string error, string? hint = null )
        {
            if( Logger == null )
                return;

            IsBindable = false;

            Logger.Error<string, string, string>( "PropertyValidation error -- {0}{1} - {2}",
                PropertyPath!,
                hint == null ? string.Empty : $" ({hint})",
                error );
        }

        private void CheckBasicBindability( Type toCheck )
        {
            var bindableInfo = BindableTypeInfo.Create( toCheck );

            if( bindableInfo.BindableType == BindableType.Unsupported )
                LogError( $"{toCheck} is neither a simple type, nor an array/list of simple types" );
        }
    }
}