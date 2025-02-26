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

using System;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine;

public class J4JCommandLineProvider : ConfigurationProvider
{
    public J4JCommandLineProvider( J4JCommandLineSource source )
    {
        Source = source;

        // watch for changes, to the command line text 
        // and the OptionsCollection
        Source.SourceChanged += OnSourceChanged;
    }

    private void OnSourceChanged( object? sender, EventArgs e )
    {
        Load();
    }

    public J4JCommandLineSource Source { get; }

    public override void Load()
    {
        if( Source.Parser == null
        || !Source.Parser.Collection.IsConfigured
        || Source.CommandLineSource == null )
            return;

        Source.Parser.Collection.ClearValues();

        if( !Source.Parser.Parse( Source.CommandLineSource.CommandLine ) )
            return;

        foreach( var option in Source.Parser.Collection.OptionsInternal )
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
                    throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{option.Style}'" );
            }
        }
    }
}
