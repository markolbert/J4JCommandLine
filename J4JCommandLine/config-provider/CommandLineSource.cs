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
using System.Threading;

namespace J4JSoftware.Configuration.CommandLine
{
    public class CommandLineSource
    {
        public event EventHandler<ConfigurationChangedEventArgs>? Changed;

        public CommandLineSource()
        {
            var temp = Environment.GetCommandLineArgs();
            CommandLine = temp.Length > 0 ? string.Join( " ", temp[ 1.. ] ) : string.Empty;
        }

        public void OptionsConfigurationChanged() => OnChanged();

        public string CommandLine { get; private set; }

        public void SetCommandLine( string newCmdLine )
        {
            CommandLine = newCmdLine;
            OnChanged( newCmdLine );
        }

        public void SetCommandLine( string[] args ) => SetCommandLine( string.Join( " ", args ) );
        public void SetCommandLine( IEnumerable<string> args ) => SetCommandLine( string.Join( " ", args ) );

        private void OnChanged( string newCommandLine ) => Changed?.Invoke( this,
            new ConfigurationChangedEventArgs { NewCommandLine = newCommandLine } );

        private void OnChanged() =>
            Changed?.Invoke( this, new ConfigurationChangedEventArgs { OptionsConfigurationChanged = true } );
    }
}