#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'ColorfulHelp' is free software: you can redistribute it
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

namespace J4JSoftware.Configuration.J4JCommandLine
{
    [ Flags ]
    public enum Borders
    {
        Left = 1 << 0,
        Top = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,

        None = 0,
        NoBottom = Left | Top | Right,
        NoTop = Left | Bottom | Right,
        NoLeft = Top | Bottom | Right,

        All = Left | Top | Right | Bottom
    }
}