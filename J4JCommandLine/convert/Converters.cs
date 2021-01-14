using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Converters : IConverters
    {
        private readonly IJ4JLogger? _logger;
        private readonly List<IConverter> _converters;
        private readonly DefaultConverter _defaultConv = new();

        public Converters( 
            IEnumerable<IConverter> converters,
            IJ4JLogger? logger 
        )
        {
            _converters = converters.ToList();
            _logger = logger;
        }

        public bool CanConvert( Type toCheck ) => _converters.Any( c => c.CanConvert( toCheck ) )
                                                  || _defaultConv.CanConvert( toCheck );

        public IConverter? GetConverter( Type toCheck ) =>
            _converters.FirstOrDefault( c => c.CanConvert( toCheck ) )
            ?? ( _defaultConv.CanConvert( toCheck ) ? _defaultConv : null );

        public object? Convert( Type targetType, IEnumerable<string> values )
        {
            var converter = GetConverter( targetType );

            return converter?.Convert( targetType, values );
        }

        public T? Convert<T>( IEnumerable<string> values )
        {
            var retVal = Convert( typeof(T), values );

            if( retVal == null )
                return default(T);

            return (T) retVal;
        }
    }
}