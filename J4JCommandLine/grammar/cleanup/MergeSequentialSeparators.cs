#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// MergeSequentialSeparators.cs
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

using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine;

public class MergeSequentialSeparators : ICleanupTokens
{
    public void Process( List<Token> tokens )
    {
        var toRemove = new List<int>();

        Token? prevToken = null;

        for( var idx = 0; idx < tokens.Count; idx++ )
        {
            var token = tokens[ idx ];

            if( token.Type == LexicalType.Separator && prevToken?.Type == token.Type )
                toRemove.Add( idx );

            prevToken = token;
        }

        tokens.RemoveRange( toRemove );
    }
}
