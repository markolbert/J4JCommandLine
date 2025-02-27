#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// OptionCollection.cs
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
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.Configuration.CommandLine;

public class OptionCollection(
    StringComparison textComparison,
    ITextConverters converters
)
{
    internal List<IOptionInternal> OptionsInternal { get; } = [];
    internal ITextConverters Converters { get; } = converters;

    public ReadOnlyCollection<IOption> Options => OptionsInternal.Cast<IOption>().ToList().AsReadOnly();
    public int Count => OptionsInternal.Count;

    public void ClearValues()
    {
        OptionsInternal.ForEach( x => x.ClearValues() );
    }

    // values associated with an option but in excess of the maximum number
    // of allowed values (which can be zero for things like switches which
    // do not accept or require values)
    public List<string> SpuriousValues { get; } = [];

    // things that parsed as valid keys but which do not match any key
    // configured in the collection (probably a sign of a misconfiguration
    // of IOptionCollection)
    public List<CommandLineArgument> UnknownKeys { get; } = [];

    // determines whether a key is being used by an existing option, honoring whatever
    // case sensitivity is in use
    public bool CommandLineKeyInUse( string key )
    {
        foreach( var option in OptionsInternal )
        {
            if( option.Keys.Any( x => x.Equals( key, textComparison ) ) )
                return true;
        }

        return false;
    }

    public IOption? this[ string key ]
    {
        get
        {
            return OptionsInternal.FirstOrDefault( opt =>
                                                       opt.IsInitialized
                                                    && opt.Keys.Any( k => string.Equals( k,
                                                                         key,
                                                                         textComparison ) ) );
        }
    }

    public bool KeysSpecified( params string[] keys )
    {
        if( keys.Length == 0 )
            return false;

        return keys.Any( k => OptionsInternal.Any( x =>
                                                       x.CommandLineKeyProvided?.Equals( k, textComparison )
                                                    ?? false ) );
    }
}
