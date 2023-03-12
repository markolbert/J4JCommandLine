// Copyright (c) 2021, 2022 Mark A. Olbert 
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

using System.Reflection;
using Serilog;

namespace J4JSoftware.Configuration.CommandLine;

public class BuiltInTextToValue<TBaseType> : TextToValue<TBaseType>
{
    private readonly MethodInfo _convMethod;

    public BuiltInTextToValue( MethodInfo convMethod,
        ILogger? logger )
        : base( logger )
    {
        _convMethod = convMethod;
    }

    protected override bool ConvertTextToValue( string text, out TBaseType? result )
    {
        result = default;

        try
        {
            result = (TBaseType?) _convMethod.Invoke( null, new object?[] { text } );
            return true;
        }
        catch
        {
            Logger?.Error( "Could not convert '{0}' to a {1}", text, typeof( TBaseType? ) );
            return false;
        }
    }
}