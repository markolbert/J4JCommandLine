using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TargetableTypeFactory : ITargetableTypeFactory
    {
        private readonly List<ITextConverter> _converters;
        private readonly IJ4JLogger? _logger;

        public TargetableTypeFactory( 
            IEnumerable<ITextConverter> converters,
            IJ4JLogger? logger
            )
        {
            _converters = converters.ToList();
            _logger = logger;

            _logger?.SetLoggedType( this.GetType() );
        }

        public ITargetableType Create( Type type )
        {
            if( type.IsArray )
            {
                var temp = new TargetableArray( type, _converters, _logger );

                return temp.IsCreatable ? temp : (ITargetableType) new UntargetableType();
            }

            if( typeof(ICollection).IsAssignableFrom( type )
                && type.IsGenericType
                && type.GenericTypeArguments.Length == 1 )
            {
                var temp = new TargetableList( type, _converters, _logger );

                return temp.IsCreatable ? temp : (ITargetableType) new UntargetableType();
            }

            var retVal = new TargetableSimpleValue( type, _converters, _logger );

            return retVal.IsCreatable ? retVal : (ITargetableType) new UntargetableType();
        }
    }
}