#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// J4JCommandLineProvider.cs
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

using System.ComponentModel;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class J4JCommandLineProvider : ConfigurationProvider
{
    private readonly IParser _parser;
    private readonly string _cmdLineText;
    private readonly ILogger? _logger = BuildTimeLoggerFactory.Default.Create<J4JCommandLineProvider>();

    internal J4JCommandLineProvider( J4JCommandLineSource source )
    {
        _parser = source.Parser;
        _cmdLineText = source.CommandLineText;
    }

    public override void Load()
    {
        if( string.IsNullOrWhiteSpace( _cmdLineText ) )
            return;

        _parser.Collection.ClearValues();

        if( !_parser.Parse( _cmdLineText ) )
            return;

        foreach( var option in _parser.Collection.OptionsInternal )
        {
            switch( option.Style )
            {
                case OptionStyle.Switch:
                    Set( option.ContextPath!,
                         string.IsNullOrEmpty( option.CommandLineKeyProvided )
                             ? "false"
                             : "true" );
                    break;

                case OptionStyle.SingleValued:
                    if( option.NumValuesAllocated != 0 )
                        Set( option.ContextPath!, option.Values[ 0 ] );
                    break;

                case OptionStyle.ConcatenatedSingleValue:
                    // concatenated single value properties (e.g., flag enums) are
                    // single valued from a target point of view (i.e., they're not
                    // collections), but they contain multiple string values from
                    // allocating the command line
                    if( option.NumValuesAllocated > 0 )
                        Set( option.ContextPath!, string.Join( ", ", option.Values ) );
                    break;

                case OptionStyle.Collection:
                    for( var idx = 0; idx < option.NumValuesAllocated; idx++ )
                    {
                        Set( $"{option.ContextPath}:{idx}", option.Values[ idx ] );
                    }

                    break;

                default:
                    _logger?.UnsupportedOptionStyle( option.Style.ToString() );
                    break;
            }
        }
    }
}
