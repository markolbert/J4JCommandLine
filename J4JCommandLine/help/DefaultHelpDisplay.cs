#region copyright
// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// DefaultHelpDisplay.cs
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

public class DefaultHelpDisplay : HelpDisplay
{
    public DefaultHelpDisplay( ILexicalElements tokens,
        OptionCollection collection )
        : base( tokens, collection )
    {
    }

    public override void Display()
    {
        Console.WriteLine( "Command line help\n" );

        foreach( var option in Collection.OptionsInternal )
        {
            Console.WriteLine( $"Keys: {string.Join( ", ", GetKeys( option ) )}" );
            Console.WriteLine( $"Description:  {option.Description}" );
            Console.WriteLine( $"Style:  {GetStyleText( option )}" );

            var defaultValue = option.GetDefaultValue();
            if( !string.IsNullOrEmpty( defaultValue ) )
                Console.WriteLine( $"Default Value:  {defaultValue}" );

            if( option.Required )
                Console.WriteLine( "Required" );

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}