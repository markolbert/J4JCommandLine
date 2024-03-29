﻿#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// ChangeObserver.cs
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

// thanx to Peter van den Hout for this!
// https://ofpinewood.com/blog/creating-a-custom-configurationprovider-for-a-entity-framework-core-source
public class ChangeObserver
{
    public event EventHandler<ConfigurationChangedEventArgs>? Changed;

    #region singleton

    private static readonly Lazy<ChangeObserver> lazy = new Lazy<ChangeObserver>( () => new ChangeObserver() );
    public static ChangeObserver Instance => lazy.Value;

    #endregion

    private ChangeObserver()
    {
    }

    public void OnChanged( string newCommandLine ) =>
        Changed?.Invoke( this, new ConfigurationChangedEventArgs { NewCommandLine = newCommandLine } );

    public void OnChanged() =>
        Changed?.Invoke( this, new ConfigurationChangedEventArgs { OptionsConfigurationChanged = true } );
}