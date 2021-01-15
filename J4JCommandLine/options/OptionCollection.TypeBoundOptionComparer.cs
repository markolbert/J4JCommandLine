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
                if( x is null) return false;
                if( y is null) return false;

                // this is messy because constructed generic types are not deemed to
                // be equal even if they are built from the same types. We are also
                // assuming the ITypeBoundOption instances are instances of
                // TypeBoundOption when they could, in reality, be instances of
                // something else entirely.
                if( !GetTypeBoundTypeParameters( x, out var xContainerType ) )
                    return false;

                if (!GetTypeBoundTypeParameters(y, out var yContainerType))
                    return false;

                return xContainerType!.IsAssignableFrom( yContainerType! );
            }

            private bool GetTypeBoundTypeParameters( ITypeBoundOption tbOption, out Type? containerType )
            {
                containerType = null;

                var tbType = tbOption.GetType();

                // we assume we were given an instance of a generic type with two type parameters
                if (!tbType.IsGenericType || tbType.GetGenericArguments().Length != 2)
                    return false;

                // we further assume we are derived from TypeBoundOption<,>
                if( !typeof(TypeBoundOption<,>).IsAssignableFrom( tbType.GetGenericTypeDefinition() ) )
                    return false;

                containerType = tbType.GetGenericArguments()[ 0 ];

                return true;
            }

            public int GetHashCode( ITypeBoundOption obj )
            {
                return obj.ContainerType.GetHashCode();
            }
        }
    }
}