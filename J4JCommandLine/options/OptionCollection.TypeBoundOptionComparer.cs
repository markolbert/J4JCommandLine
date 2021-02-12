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

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class OptionCollection
    {
        private class TypeBoundOptionComparer : IEqualityComparer<ITypeBoundOption>
        {
            public bool Equals( ITypeBoundOption? x, ITypeBoundOption? y )
            {
                if( ReferenceEquals( x, y ) ) return true;
                if( x is null ) return false;
                if( y is null ) return false;

                // this is messy because constructed generic types are not deemed to
                // be equal even if they are built from the same types. We are also
                // assuming the ITypeBoundOption instances are instances of
                // TypeBoundOption when they could, in reality, be instances of
                // something else entirely.
                if( !GetTypeBoundTypeParameters( x, out var xContainerType ) )
                    return false;

                if( !GetTypeBoundTypeParameters( y, out var yContainerType ) )
                    return false;

                return xContainerType!.IsAssignableFrom( yContainerType! );
            }

            public int GetHashCode( ITypeBoundOption obj )
            {
                return obj.ContainerType.GetHashCode();
            }

            private bool GetTypeBoundTypeParameters( ITypeBoundOption tbOption, out Type? containerType )
            {
                containerType = null;

                var tbType = tbOption.GetType();

                // we assume we were given an instance of a generic type with two type parameters
                if( !tbType.IsGenericType || tbType.GetGenericArguments().Length != 2 )
                    return false;

                // we further assume we are derived from TypeBoundOption<,>
                if( !typeof(TypeBoundOption<,>).IsAssignableFrom( tbType.GetGenericTypeDefinition() ) )
                    return false;

                containerType = tbType.GetGenericArguments()[ 0 ];

                return true;
            }
        }
    }
}