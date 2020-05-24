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
            if( !HasPublicParameterlessConstructor )
            {
                if( type == typeof(string) )
                    IsCreatable = true;
                else logger?.Error<Type>( "{0} has no public parameterless constructor and is not a string", type );
            }
            else
            {
                if( type != typeof(string) && converters.All(c=>c.SupportedType != type))
                    logger?.Error<Type>("{0} is not convertible from text", type);
                else IsCreatable = true;
            }
        }

        public override object? Create() =>
            SupportedType == typeof(string) ? string.Empty : ParameterlessConstructor?.Invoke( null );
    }
}