using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    // represents an Array of some Type. Can only be created via the ITargetedPropertyFactory interface
    public class TargetableArray : TargetableType
    {
        internal TargetableArray(
            Type type,
            List<ITextConverter> converters,
            IJ4JLogger? logger
        )
            : base( type.GetElementType()!, Multiplicity.Array )
        {
            if( !type.IsArray )
                logger?.Error<Type>( "{0} is not an Array", type );
            else
            {
                if( type.GetArrayRank() != 1 )
                    logger?.Error<Type>( "{0} is not a one-dimensional Array", type );
                else
                {
                    var elementType = type.GetElementType()!;

                    Converter = converters.FirstOrDefault( c => c.SupportedType == elementType );

                    if( Converter != null )
                        IsCreatable = true;
                    else logger?.Error<Type>( "{0} is not convertible from text", elementType );
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