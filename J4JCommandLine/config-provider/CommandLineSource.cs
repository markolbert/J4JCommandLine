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

using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine
{
    public class CommandLineSource
    {
        private readonly ChangeObserver _changeObserver = ChangeObserver.Instance;

        public CommandLineSource( string cmdLine )
        {
            CommandLine = cmdLine;
            IsFixed = true;
        }

        public CommandLineSource( string[] args )
            : this( string.Join( " ", args ) )
        {
        }

        public CommandLineSource( IEnumerable<string> args )
            : this(string.Join(" ", args))
        {
        }

        public CommandLineSource()
        {
        }

        public bool IsFixed { get; }
        public string CommandLine { get; private set; } = string.Empty;

        public bool SetValue( string newCmdLine )
        {
            if( IsFixed )
                return false;

            CommandLine = newCmdLine;
            _changeObserver.OnChanged( newCmdLine );

            return true;
        }
    }
}