#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// CommandLineSource.cs
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

public class CommandLineSource
{
    public event EventHandler<ConfigurationChangedEventArgs>? Changed;

    public CommandLineSource( StringComparison textComparison )
    {
        // we don't want the name of the executable so we need to remove it
        // it's the first argument in what gets returned by Environment.GetCommandLineArgs()
        var temp = Environment.GetCommandLineArgs();

        var cmdLine = Environment.CommandLine;
        CommandLine = cmdLine.IndexOf( temp[ 0 ], textComparison ) == 0
            ? cmdLine.Replace( temp[ 0 ], string.Empty )
            : cmdLine;
    }

    public void OptionsConfigurationChanged() => OnChanged();

    public string CommandLine { get; private set; }

    public void SetCommandLine( string newCmdLine )
    {
        CommandLine = newCmdLine;
        OnChanged( newCmdLine );
    }

    public void SetCommandLine( string[] args ) => SetCommandLine( string.Join( " ", args ) );
    public void SetCommandLine( IEnumerable<string> args ) => SetCommandLine( string.Join( " ", args ) );

    private void OnChanged( string newCommandLine ) =>
        Changed?.Invoke( this,
                         new ConfigurationChangedEventArgs { NewCommandLine = newCommandLine } );

    private void OnChanged() =>
        Changed?.Invoke( this, new ConfigurationChangedEventArgs { OptionsConfigurationChanged = true } );
}