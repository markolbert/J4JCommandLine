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
        private readonly List<ITextToValue> _converters;
        private readonly IJ4JLogger? _logger;

        public Converters(
            IEnumerable<ITextToValue> converters,
            IJ4JLogger? logger
        )
        {
            _converters = converters.ToList();
            _converters.AddRange( BuiltInTextToValue.SystemConverters );

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool CanConvert( Type toCheck ) => _converters.Any( c => c.TargetType == toCheck );

        public bool Convert( Type targetType, IEnumerable<string> values, out object? result )
        {
            var valueList = values.ToList();

            foreach( var converter in _converters
                .Where( c => c.TargetType == targetType )
                .OrderByDescending( x => x.Priority ) )
            {
                if( converter.Convert( valueList, out result ) )
                    return true;
            }

            result = null;

            return false;
        }

        public bool Convert<T>( IEnumerable<string> values, out T? result )
        {
            result = default;

            if( !Convert( typeof(T), values, out var innerResult ) )
                return false;

            try
            {
                result = (T?) innerResult;
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}