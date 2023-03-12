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

using System;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine;

public class J4JCommandLineSource : IConfigurationSource
{
    public event EventHandler? SourceChanged;

    ///TODO: cleanupTokens not used
    public J4JCommandLineSource(
        IParser parser,
        params ICleanupTokens[] cleanupTokens
        )
    {
        Parser = parser;
        CommandLineSource = Initialize();
    }

    public J4JCommandLineSource( 
        IParser parser
        )
    {
        Parser = parser;
        CommandLineSource = Initialize();
    }

    private CommandLineSource? Initialize()
    {
        if( Parser == null )
            return null;

        Parser.Collection.Configured += Options_Configured;

        var retVal = new CommandLineSource( Parser.Tokenizer.TextComparison );
        retVal.Changed += OnCommandLineSourceChanged;

        return retVal;
    }

    private void Options_Configured( object? sender, EventArgs e ) =>
        SourceChanged?.Invoke( this, EventArgs.Empty );

    private void OnCommandLineSourceChanged( object? sender, ConfigurationChangedEventArgs e ) =>
        SourceChanged?.Invoke( this, EventArgs.Empty );

    public CommandLineSource? CommandLineSource { get; }
    public IParser? Parser { get; }

    public IConfigurationProvider Build( IConfigurationBuilder builder )
    {
        return new J4JCommandLineProvider( this );
    }
}