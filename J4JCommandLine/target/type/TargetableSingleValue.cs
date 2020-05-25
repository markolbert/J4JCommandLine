using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TargetableSingleValue : TargetableType
    {
        internal TargetableSingleValue( 
            Type type, 
            List<ITextConverter> converters,
            IJ4JLogger? logger
            )
            : base( type, PropertyMultiplicity.SingleValue )
        {
            Converter = converters.FirstOrDefault( c => c.SupportedType == type );

            if( type.IsValueType || type == typeof( string ) )
            {
                if( Converter != null )
                    IsCreatable = true;
                else
                    logger?.Error<Type>( "{0} does not have an ITextConverter available", type );
            }
            else
            {
                if( HasPublicParameterlessConstructor )
                {
                    if( Converter != null )
                        IsCreatable = true;
                    else
                        logger?.Error<Type>( "{0} does not have an ITextConverter available", type );
                }
                else
                    logger?.Error<Type>(
                        "{0} has no public parameterless constructor and is not a string or a ValueType", type );
            }
        }

        public override object? Create() =>
            SupportedType == typeof(string) ? string.Empty : ParameterlessConstructor?.Invoke( null );
    }
}