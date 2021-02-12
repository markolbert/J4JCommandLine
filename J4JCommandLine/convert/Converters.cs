#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Converters : IConverters
    {
        private readonly List<IConverter> _converters;
        private readonly DefaultConverter _defaultConv = new();
        private readonly IJ4JLogger? _logger;

        public Converters(
            IEnumerable<IConverter> converters,
            IJ4JLogger? logger
        )
        {
            _converters = converters.ToList();
            _logger = logger;
        }

        public bool CanConvert( Type toCheck )
        {
            return _converters.Any( c => c.CanConvert( toCheck ) )
                   || _defaultConv.CanConvert( toCheck );
        }

        public object? Convert( Type targetType, IEnumerable<string> values )
        {
            var converter = GetConverter( targetType );

            return converter?.Convert( targetType, values );
        }

        public T? Convert<T>( IEnumerable<string> values )
        {
            var retVal = Convert( typeof(T), values );

            if( retVal == null )
                return default;

            return (T) retVal;
        }

        public IConverter? GetConverter( Type toCheck )
        {
            return _converters.FirstOrDefault( c => c.CanConvert( toCheck ) )
                   ?? ( _defaultConv.CanConvert( toCheck ) ? _defaultConv : null );
        }
    }
}