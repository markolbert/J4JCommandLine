using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine.Deprecated
{
    // represents an Array of some Type. Can only be created via the ITargetedPropertyFactory interface
    public class TargetableArray : TargetableType
    {
        internal TargetableArray( Type type, List<ITextConverter> converters )
            : base( type.GetElementType()!, PropertyMultiplicity.Array )
        {
            if( type.IsArray )
            {
                if( type.GetArrayRank() == 1 )
                {
                    var elementType = type.GetElementType()!;

                    Converter = converters.FirstOrDefault( c => c.SupportedType == elementType );

                    if( Converter != null )
                        IsCreatable = true;
                }
            }
        }

        // Returns null if the object described by the instance is not creatable. Otherwise
        // creates an empty/zero-length Array of SupportedType
        public override object? GetDefaultValue()
        {
            if (!IsCreatable)
                return null;

            return Array.CreateInstance( SupportedType, 0 );
        }
    }
}