#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ITextConverters.cs
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

using System;
using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine;

public interface ITextConverters : IReadOnlyDictionary<Type, ITextToValue>
{
    bool AddConverter( ITextToValue converter, bool replaceExisting = false );
    bool AddConverters( IEnumerable<ITextToValue> converters, bool replaceExisting = false );

    ITextToValue? GetConverter( Type simpleType );

    bool CanConvert( Type toCheck );
    bool Convert( Type targetType, IEnumerable<string> textValues, out object? result );
}