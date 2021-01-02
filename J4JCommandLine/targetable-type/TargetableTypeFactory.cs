using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine.Deprecated
{
    // implements ITargetableTypeFactory to create supported ITargetableType objects.
    // if you expand the framework's repertoire beyond simple values, arrays and generic
    // lists you'll have to implement your own ITargetableTypeFactory and use it instead
    // of this one.
    public class TargetableTypeFactory : ITargetableTypeFactory
    {
        private readonly List<ITextConverter> _converters;

        public TargetableTypeFactory( IEnumerable<ITextConverter> converters )
        {
            _converters = converters.ToList();
        }

        public ITargetableType Create( Type type )
        {
            if( type.IsArray )
            {
                var temp = new TargetableArray( type, _converters );

                return temp.IsCreatable ? temp : (ITargetableType) new UntargetableType();
            }

            if( typeof(ICollection).IsAssignableFrom( type )
                && type.IsGenericType
                && type.GenericTypeArguments.Length == 1 )
            {
                var temp = new TargetableList( type, _converters );

                return temp.IsCreatable ? temp : (ITargetableType) new UntargetableType();
            }

            var retVal = new TargetableSimpleValue( type, _converters );

            return retVal.IsCreatable ? retVal : (ITargetableType) new UntargetableType();
        }
    }
}