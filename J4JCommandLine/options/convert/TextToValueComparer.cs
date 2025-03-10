﻿#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TextToValueComparer.cs
//
// This file is part of JumpForJoy Software's J4JCommandLine.
// 
// J4JCommandLine is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JCommandLine is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JCommandLine. If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine;

public sealed class TextToValueComparer : IEqualityComparer<ITextToValue>
{
    public bool Equals( ITextToValue? x, ITextToValue? y )
    {
        if( ReferenceEquals( x, y ) ) return true;
        if( ReferenceEquals( x, null ) ) return false;
        if( ReferenceEquals( y, null ) ) return false;
        if( x.GetType() != y.GetType() ) return false;

        return x.TargetType == y.TargetType;
    }

    public int GetHashCode( ITextToValue obj ) => obj.TargetType.GetHashCode();
}
