#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.J4JCommandLine' is free software: you can redistribute it
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

using System.Collections.Generic;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;

namespace J4JSoftware.Binder.Tests
{
    public static class TestExtensions
    {
        public static void CreateOptionsFromContextKeys(
            this IOptionCollection options,
            IEnumerable<OptionConfig> optConfigs )
        {
            foreach( var optConfig in optConfigs ) options.CreateOptionFromContextKey( optConfig );
        }

        private static IOptionCollection CreateOptionFromContextKey( this IOptionCollection options,
            OptionConfig optConfig )
        {
            var option = options.Add( optConfig.GetPropertyType(), optConfig.ContextPath );
            option.Should().NotBeNull();

            option!.AddCommandLineKey( optConfig.CommandLineKey )
                .SetStyle( optConfig.Style );

            if( optConfig.Required ) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return options;
        }
    }
}