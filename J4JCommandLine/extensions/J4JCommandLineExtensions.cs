#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// J4JCommandLineExtensions.cs
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public static class J4JCommandLineExtensions
{
    private static readonly ILogger? Logger =
        CommandLineLoggerFactory.Default.Create( typeof( J4JCommandLineExtensions ) );

    public static IConfigurationBuilder AddJ4JCommandLine(
        this IConfigurationBuilder builder,
        J4JCommandLineBuilder optionBuilder,
        ref ILexicalElements? tokens
    )
    {
        var optionsGenerator = new OptionsGenerator( optionBuilder );

        tokens ??= optionBuilder.Os switch
        {
            CommandLineOperatingSystems.Windows => new WindowsLexicalElements( optionBuilder ),
            CommandLineOperatingSystems.Linux => new LinuxLexicalElements( optionBuilder ),
            _ => null
        };

        if( tokens == null )
        {
            Logger?.UndefinedLexicalElements();
            return builder;
        }

        var tokenizer = new Tokenizer( tokens, optionBuilder );
        var parsingTable = new ParsingTable( optionsGenerator );
        var parser = new Parser( optionBuilder, parsingTable, tokenizer );

        var source = new J4JCommandLineSource( parser );

        builder.Add( source );

        return builder;
    }

    // this method should only be used for testing purposes
    // as normally the command line is pulled from the 
    // environment
    public static IConfigurationBuilder AddJ4JCommandLine(
        this IConfigurationBuilder builder,
        J4JCommandLineBuilder optionBuilder,
        string cmdLineText,
        ref ILexicalElements? tokens
    )
    {
        var optionsGenerator = new OptionsGenerator( optionBuilder );

        tokens ??= optionBuilder.Os switch
        {
            CommandLineOperatingSystems.Windows => new WindowsLexicalElements( optionBuilder ),
            CommandLineOperatingSystems.Linux => new LinuxLexicalElements( optionBuilder ),
            _ => null
        };

        if( tokens == null )
        {
            Logger?.UndefinedLexicalElements();
            return builder;
        }

        var tokenizer = new Tokenizer( tokens, optionBuilder );
        var parsingTable = new ParsingTable( optionsGenerator );
        var parser = new Parser( optionBuilder, parsingTable, tokenizer );

        var source = new J4JCommandLineSource( parser, cmdLineText );

        builder.Add( source );

        return builder;
    }

    internal static TypeNature GetTypeNature( this Type toCheck )
    {
        if( toCheck.IsArray )
        {
            var elementType = toCheck.GetElementType();

            return elementType == null
                ? TypeNature.Unsupported
                : TypeNature.Array;
        }

        // if it's not an array and not a generic it's a "simple" type
        if( !toCheck.IsGenericType )
            return TypeNature.Simple;

        if( toCheck.GenericTypeArguments.Length != 1 )
            return TypeNature.Unsupported;

        var genType = toCheck.GetGenericArguments()[ 0 ];

        return typeof( List<> ).MakeGenericType( genType ).IsAssignableFrom( toCheck )
            ? TypeNature.List
            : TypeNature.Unsupported;
    }

    internal static Type? GetTargetType( this Type toCheck )
    {
        if( toCheck.IsArray )
            return toCheck.GetElementType();

        // if it's not an array and not a generic it's a "simple" type
        if( !toCheck.IsGenericType )
            return toCheck;

        if( toCheck.GenericTypeArguments.Length != 1 )
            return null;

        var genType = toCheck.GetGenericArguments()[ 0 ];

        return typeof( List<> ).MakeGenericType( genType ).IsAssignableFrom( toCheck ) ? genType : null;
    }
}
