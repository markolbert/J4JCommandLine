#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// LexicalElements.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class LexicalElements : ILexicalElements
{
    private readonly Dictionary<LexicalType, List<Token>> _available = new();

    public LexicalElements(
        StringComparison textComparison,
        ILoggerFactory? loggerFactory = null,
        bool inclCommon = true
    )
    {
        TextComparison = textComparison;
        Logger = loggerFactory?.CreateLogger( GetType() );

        if( !inclCommon )
            return;

        Add( LexicalType.Separator, " " );
        Add( LexicalType.Separator, "\t" );
        Add( LexicalType.ValuePrefix, "=" );
    }

    protected ILogger? Logger { get; }

    public StringComparison TextComparison { get; }

    public int Count => _available.Count;

    public bool Add( LexicalType type, string text )
    {
        if( type == LexicalType.Text || type == LexicalType.StartOfInput )
        {
            Logger?.LogError( "Cannot include {0} tokens", type );
            return false;
        }

        if( _available.SelectMany( kvp => kvp.Value )
                      .Any( t => t.Text.Equals( text, TextComparison ) ) )
        {
            Logger?.LogError( "Duplicate token text '{0}' ({1})", text, type );
            return false;
        }

        var newToken = new Token( type, text );

        if( _available.TryGetValue( type, out var value ) )
            value.Add( newToken );
        else _available.Add( type, [newToken] );

        return true;
    }

    public IEnumerator<Token> GetEnumerator()
    {
        foreach( var kvp in _available )
        {
            foreach( var token in kvp.Value )
            {
                yield return token;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
