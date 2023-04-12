#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// RawCommandLine.cs
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
using System.Runtime.InteropServices;

namespace J4JSoftware.Configuration.CommandLine.Deprecate;

public class RawCommandLine
{
    private readonly bool _useKernel32;

    public RawCommandLine()
    {
        _useKernel32 = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT      => true,
            PlatformID.Win32S       => true,
            PlatformID.Win32Windows => true,
            PlatformID.WinCE        => true,
            _                       => false
        };
    }

    [ DllImport( "kernel32.dll", CharSet = CharSet.Auto ) ]
    private static extern IntPtr GetCommandLine();

    public string GetRawCommandLine()
    {
        if( !_useKernel32 )
            return Environment.CommandLine;

        var ptr = GetCommandLine();

        return Marshal.PtrToStringAuto( ptr )!;
    }
}