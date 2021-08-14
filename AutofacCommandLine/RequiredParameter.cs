#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'AutofacCommandLine' is free software: you can redistribute it
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
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.Configuration.CommandLine
{
    public class RequiredParameter
    {
        private RequiredParameter(Type requiredType)
        {
            ParameterType = requiredType;
        }

        public Type ParameterType { get; }
        public bool IsCompatible { get; private set; }

        public bool CheckConstructorParameter( Type ctorParamType )
        {
            IsCompatible = false;

            return IsCompatible;
        }

        public class RequiredParameters
        {
            private readonly List<RequiredParameter> _reqdParameters;

            public RequiredParameters(Type[] requiredTypes)
            {
                _reqdParameters = requiredTypes
                    .Select( t => new RequiredParameter( t ) )
                    .ToList();
            }

            public ReadOnlyCollection<RequiredParameter> Parameters => _reqdParameters.AsReadOnly();

            public bool ParameterCountMatches { get; private set; }
            public bool HasPublicConstructors { get; private set; }
            public bool HasCompatiblePublicConstructor => ParameterCountMatches 
                                                          && _reqdParameters.All( x => x.IsCompatible );

            public bool CheckConstructor(Type toCheck)
            {
                ParameterCountMatches = false;
                HasPublicConstructors = false;

                foreach( var ctor in toCheck.GetConstructors() )
                {
                    _reqdParameters.ForEach(x => x.IsCompatible = false);

                    var ctorParams = ctor.GetParameters();

                    if( ctorParams.Length != _reqdParameters.Count )
                        continue;

                    for( var idx = 0; idx < ctorParams.Length; idx++ )
                    {
                    }
                }


                return HasCompatiblePublicConstructor;
            }
        }
    }
}