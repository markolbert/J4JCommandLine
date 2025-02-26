#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// BuiltInTextToValue.cs
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

using System.Reflection;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class BuiltInTextToValue<TBaseType>(
    MethodInfo convMethod
) : TextToValue<TBaseType>
{
    protected override bool ConvertTextToValue( string text, out TBaseType? result )
    {
        result = default;

        try
        {
            result = (TBaseType?) convMethod.Invoke( null, [text] );
            return true;
        }
        catch
        {
            Logger?.ConversionFailed( typeof( TBaseType? ).Name, text );
            return false;
        }
    }
}
