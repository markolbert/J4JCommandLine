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
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public sealed class LinuxLexicalElements : LexicalElements
{
    public LinuxLexicalElements( IJ4JLogger? logger = null )
        : base( StringComparison.Ordinal, logger )
    {
        Add( LexicalType.Quoter, "\"" );
        Add( LexicalType.Quoter, "'" );
        Add( LexicalType.KeyPrefix, "-" );
        Add( LexicalType.KeyPrefix, "--" );
    }
}