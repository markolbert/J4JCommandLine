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
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class BindabilityValidator
    {
        public enum BuiltInConverters
        {
            DoNotAdd,
            AddAtInitialization,
            AddDynamically
        }

        private record BuiltInConverter( Type ReturnType, MethodInfo MethodInfo );

        private static List<ITextToValue> GetBuiltInConverters( IJ4JLogger? logger )
        {
            var retVal = new List<ITextToValue>();

            foreach( var builtInConverter in GetBuiltInTargetTypes() )
            {
                var builtInType = typeof(BuiltInTextToValue<>).MakeGenericType( builtInConverter.ReturnType );

                retVal.Add( (ITextToValue)Activator.CreateInstance(
                        builtInType,
                        new object?[] { builtInConverter.MethodInfo, logger } )!
                );
            }

            return retVal;
        }

        private static List<BuiltInConverter> GetBuiltInTargetTypes() =>
            typeof(Convert)
                .GetMethods( BindingFlags.Static | BindingFlags.Public )
                .Where( m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 && !typeof(string).IsAssignableFrom( parameters[ 0 ].ParameterType );
                } )
                .Select( x => new BuiltInConverter( x.ReturnType, x ) )
                .ToList();
    }
}