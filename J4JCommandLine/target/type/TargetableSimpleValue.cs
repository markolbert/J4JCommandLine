using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // represents an 'simple' Type, meaning one which isn't either an array or a List<>.
    // Can only be created via the ITargetedPropertyFactory interface
    public class TargetableSimpleValue : TargetableType
    {
        internal TargetableSimpleValue( Type type, List<ITextConverter> converters )
            : base( type, Multiplicity.SimpleValue )
        {
            Converter = converters.FirstOrDefault( c => c.SupportedType == type );

            if( type.IsValueType || type == typeof(string) )
                IsCreatable = Converter != null;
            else
                IsCreatable = HasPublicParameterlessConstructor;
        }

        // strings aren't created; instead, an empty string is returned. Returns null if the object
        // described by the instance is not creatable
        public override object? GetDefaultValue()
        {
            if( !IsCreatable )
                return null;

            if( SupportedType == typeof( string ) )
                return string.Empty;

            return Activator.CreateInstance( SupportedType );
        }
    }
}