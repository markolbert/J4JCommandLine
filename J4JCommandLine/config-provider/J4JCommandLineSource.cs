#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// J4JCommandLineSource.cs
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
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine;

internal class J4JCommandLineSource : IConfigurationSource
{
    internal J4JCommandLineSource(
        IParser parser
    )
    {
        Parser = parser;

        // we don't want the name of the executable so we need to remove it.
        // it's the first argument in what gets returned by Environment.GetCommandLineArgs()
        var temp = Environment.GetCommandLineArgs();

        var cmdLine = Environment.CommandLine;
        CommandLineText = cmdLine.IndexOf( temp[ 0 ], parser.Tokenizer.TextComparison ) == 0
            ? cmdLine.Replace( temp[ 0 ], string.Empty )
            : cmdLine;
    }

    internal J4JCommandLineSource(
        IParser parser,
        string cmdLineText
    )
    {
        Parser = parser;
        CommandLineText = cmdLineText;
    }

    public IParser Parser { get; }
    public string CommandLineText { get; }

    public IConfigurationProvider Build( IConfigurationBuilder builder ) => new J4JCommandLineProvider( this );
}
