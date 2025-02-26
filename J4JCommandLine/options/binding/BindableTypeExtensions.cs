#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// BindableTypeExtensions.cs
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
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine;

internal static class BindableTypeExtensions
{
    internal static bool IsFlaggedEnum( this Type toCheck ) =>
        toCheck.IsEnum && toCheck.GetCustomAttribute<FlagsAttribute>() != null;

    internal static bool IsBindableCollection( this Type toCheck )
    {
        if( toCheck.IsArray )
            return true;

        if( !toCheck.IsGenericType )
            return false;

        var typeArgs = toCheck.GetGenericArguments();

        return typeArgs.Length == 1
         && typeof( List<> ).MakeGenericType( typeArgs[ 0 ] ).IsAssignableFrom( toCheck );
    }

    internal static Type? GetBindableCollectionElement( this Type toCheck )
    {
        if( !toCheck.IsBindableCollection() )
            return null;

        return toCheck.IsArray ? toCheck.GetElementType() : toCheck.GetGenericArguments()[ 0 ];
    }
}
