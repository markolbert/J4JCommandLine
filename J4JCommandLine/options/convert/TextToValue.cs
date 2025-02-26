#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// TextToValue.cs
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
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public abstract class TextToValue<TBaseType>( ILoggerFactory? loggerFactory = null ) : ITextToValue
{
    protected abstract bool ConvertTextToValue( string text, out TBaseType? result );

    protected ILogger? Logger { get; } = loggerFactory?.CreateLogger<TBaseType>();

    public Type TargetType => typeof( TBaseType );

    public bool CanConvert( Type toCheck ) =>
        toCheck.GetTypeNature() != TypeNature.Unsupported
     && toCheck.IsAssignableFrom( TargetType );

    // targetType must be one of:
    // - TBaseType
    // - TBaseType[]
    // - List<TBaseType>
    // anything else will cause a conversion failure
    public bool Convert( Type targetType, IEnumerable<string> values, out object? result )
    {
        result = null;
        var retVal = false;

        var valueList = values.ToList();

        switch( targetType.GetTypeNature() )
        {
            case TypeNature.Simple:
                if( valueList.Count > 1 )
                {
                    Logger?.LogError( "Cannot convert multiple text values to a single value of '{0}'",
                                      typeof( TBaseType ) );

                    return false;
                }

                retVal = ConvertToSingleValue( valueList.Count == 0 ? null : valueList[ 0 ], out var singleResult );
                result = singleResult;

                break;

            case TypeNature.Array:
                retVal = ConvertToArray( valueList, out var arrayResult );
                result = arrayResult;

                break;

            case TypeNature.List:
                retVal = ConvertToArray( valueList, out var listResult );
                result = listResult;

                break;
        }

        return retVal;
    }

    // TConvType must be one of:
    // - TBaseType
    // - TBaseType[]
    // - List<TBaseType>
    // anything else will cause a conversion failure
    public bool Convert<TConvType>( IEnumerable<string> values, out TConvType? result )
    {
        result = default;

        if( !Convert( typeof( TConvType ), values, out var temp ) )
            return false;

        result = (TConvType?) temp;

        return true;
    }

    private bool ConvertToSingleValue( string? text, out TBaseType? result )
    {
        result = default;

        // null or empty strings return the default value for TBaseType,
        // whatever that may be
        if( string.IsNullOrEmpty( text ) )
            return true;

        return ConvertTextToValue( text, out result );
    }

    private bool ConvertToArray( List<string> values, out TBaseType?[]? result )
    {
        result = null;

        var retVal = new List<TBaseType?>();

        foreach( var value in values )
        {
            if( ConvertToSingleValue( value, out var temp ) )
                retVal.Add( temp );
            else
            {
                Logger?.LogError( "Could not convert '{0}' to an instance of {1}", value, typeof( TBaseType ) );
                return false;
            }
        }

        result = retVal.ToArray();

        return true;
    }
}
