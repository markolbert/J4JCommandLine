using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
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

        public override object? GetDefaultValue() => Array.CreateInstance( SupportedType, 0 );
    }
}