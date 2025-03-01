#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// HelpDisplay.cs
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public abstract class HelpDisplay : IHelpDisplay
{
    protected HelpDisplay(ILexicalElements tokens,
        OptionCollection collection)
    {
        Tokens = tokens;
        Collection = collection;
        Logger = BuildTimeLoggerFactory.Default.Create(GetType());
    }

    protected ILexicalElements Tokens { get; }
    protected ILogger? Logger { get; }
    protected OptionCollection Collection { get; }

    public abstract void Display();

    protected List<string> GetKeys( IOption option )
    {
        var retVal = new List<string>();

        foreach( var prefix in Tokens.Where( x => x.Type == LexicalType.KeyPrefix ) )
        {
            foreach( var key in option.Keys )
            {
                retVal.Add( $"{prefix.Text}{key}" );
            }
        }

        return retVal;
    }

    protected string GetStyleText( IOption option )
    {
        var reqdText = option.Required ? "must" : "can";

        switch( option.Style )
        {
            case OptionStyle.Collection:
                var sb = new StringBuilder();

                sb.Append( option.MaxValues == int.MaxValue ? $"one to {option.MaxValues:n0}" : "one or more" );
                sb.Append( $" values {reqdText} be specified" );

                return sb.ToString();

            case OptionStyle.ConcatenatedSingleValue:
                return $"one or more related values (e.g., flagged enums) {reqdText} be specified";

            case OptionStyle.SingleValued:
                return $"a single value {reqdText} be specified";

            case OptionStyle.Switch:
                return "any value specified will be ignored";
        }

        Logger?.UnsupportedOptionStyle(option.Style.ToString());

        return $"unsupported option style!";
    }
}
