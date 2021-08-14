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
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class ConstructorTester<T> : ConstructorTesterBase<T>
        where T : class
    {
        public static ConstructorTester<T> ParameterLess { get; } = new();
        public static ConstructorTester<T> RequiresLogger { get; } = new(typeof(IJ4JLogger));
        public static ConstructorTester<T> RequiresLoggerFactory { get; } = new(typeof(IJ4JLoggerFactory));

        public ConstructorTester(
            params Type[] reqdParameters
        )
            : base( reqdParameters, false )
        {
        }

        protected override IEnumerable<IEnumerable<Type>> EnumerateParameterList()
        {
            yield return RequiredParameters;
        }
    }
}