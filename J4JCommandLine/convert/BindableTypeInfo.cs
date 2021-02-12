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

#pragma warning disable 8618

namespace J4JSoftware.Configuration.CommandLine
{
    public class BindableTypeInfo
    {
        private BindableTypeInfo()
        {
        }

        public BindableType BindableType { get; private set; }
        public Type TargetType { get; private set; }

        public static BindableTypeInfo Create( Type toCheck )
        {
            if( toCheck.IsArray )
                return new BindableTypeInfo
                {
                    BindableType = BindableType.Array,
                    TargetType = toCheck.GetElementType()!
                };

            // if it's not an array and not a generic it's a "simple" type
            if( !toCheck.IsGenericType )
                return new BindableTypeInfo
                {
                    BindableType = BindableType.Simple,
                    TargetType = toCheck
                };

            if( toCheck.GenericTypeArguments.Length != 1 )
                return new BindableTypeInfo
                {
                    BindableType = BindableType.Unsupported,
                    TargetType = toCheck
                };

            var genType = toCheck.GetGenericArguments()[ 0 ];

            return typeof(List<>).MakeGenericType( genType ).IsAssignableFrom( toCheck )
                ? new BindableTypeInfo
                {
                    BindableType = BindableType.List,
                    TargetType = genType
                }
                : new BindableTypeInfo
                {
                    BindableType = BindableType.Unsupported,
                    TargetType = toCheck
                };
        }
    }
}