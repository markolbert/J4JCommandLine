﻿// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of J4JCommandLine.
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

using System;
using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine;

public class UndefinedTextToValue : ITextToValue
{
    public Type TargetType => typeof( object );
    public bool CanConvert( Type toCheck ) => false;

    public bool Convert( Type targetType, IEnumerable<string> values, out object? result )
    {
        result = null;
        return false;
    }

    public bool Convert<T>( IEnumerable<string> values, out T? result )
    {
        result = default;
        return false;
    }
}
