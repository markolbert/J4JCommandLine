#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TextToEnum.cs
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

namespace J4JSoftware.Configuration.CommandLine;

public class TextToEnum<TEnum> : TextToValue<TEnum>
    where TEnum : Enum
{
    protected override bool ConvertTextToValue( string text, out TEnum? result )
    {
        result = default;

        if( !Enum.TryParse( typeof( TEnum ), text, true, out var tempResult ) )
            return false;

        result = (TEnum?) tempResult;

        return true;
    }
}
