using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TargetableList : TargetableType
    {
        internal TargetableList(
            Type type, 
            List<ITextConverter> converters,
            IJ4JLogger? logger 
            )
            : base(type, PropertyMultiplicity.List)
        {
            if (!typeof(ICollection).IsAssignableFrom(type) )
                logger?.Error<Type>("{0} does not implement ICollection", type);
            else
            {
                if( !type.IsGenericType )
                    logger?.Error<Type>("{0} is not a generic type", type);
                else
                {
                    if( type.GenericTypeArguments.Length != 1 )
                        logger?.Error<Type>( "{0} has more than one generic type parameter", type );
                    else
                    {
                        Converter = converters.FirstOrDefault( c => c.SupportedType == type.GenericTypeArguments[ 0 ] );

                        if( Converter == null )
                            logger?.Error<Type>( "{0} is not convertible from text", type.GenericTypeArguments[ 0 ] );
                        else IsCreatable = HasPublicParameterlessConstructor;
                    }
                }
            }
        }

        public override object? GetDefaultValue()
        {
            var retType = typeof(List<>).MakeGenericType(SupportedType.GenericTypeArguments[0]);

            return Activator.CreateInstance(retType)!;
        }
    }
}