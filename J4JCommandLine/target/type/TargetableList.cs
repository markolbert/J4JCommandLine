using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // represents a generic List<> of some Type. Can only be created via the ITargetedPropertyFactory interface
    public class TargetableList : TargetableType
    {
        internal TargetableList( Type type, List<ITextConverter> converters )
            : base(type, Multiplicity.List)
        {
            if (typeof(ICollection).IsAssignableFrom(type) )
            {
                if( type.IsGenericType )
                {
                    if( type.GenericTypeArguments.Length == 1 )
                    {
                        Converter = converters.FirstOrDefault( c => c.SupportedType == type.GenericTypeArguments[ 0 ] );

                        if( Converter != null )
                            IsCreatable = HasPublicParameterlessConstructor;
                    }
                }
            }
        }

        // Returns null if the object described by the instance is not creatable. Otherwise
        // creates an empty List<SupportedType>
        public override object? GetDefaultValue()
        {
            if( !IsCreatable )
                return null;

            var retType = typeof(List<>).MakeGenericType(SupportedType.GenericTypeArguments[0]);

            return Activator.CreateInstance(retType)!;
        }
    }
}