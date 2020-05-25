using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TargetableSimpleValue : TargetableType
    {
        internal TargetableSimpleValue( 
            Type type, 
            List<ITextConverter> converters,
            IJ4JLogger? logger
            )
            : base( type, Multiplicity.SimpleValue )
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
                    IsCreatable = true;
                else
                    logger?.Error<Type>(
                        "{0} has no public parameterless constructor and is not a string or a ValueType", type );
            }
        }

        public override object? GetDefaultValue()
        {
            if( SupportedType == typeof( string ) )
                return string.Empty;

            //if( SupportedType.IsValueType )
            return Activator.CreateInstance( SupportedType );

            //return ParameterlessConstructor?.Invoke( null );
        }
    }
}