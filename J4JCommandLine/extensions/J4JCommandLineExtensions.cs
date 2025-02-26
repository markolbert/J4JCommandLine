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
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.Configuration.CommandLine;

public static class J4JCommandLineExtensions
{
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

    public static IConfigurationBuilder AddJ4JCommandLine(
        this IConfigurationBuilder builder,
        IServiceProvider svcProvider,
        out OptionCollection? options,
        out CommandLineSource? cmdLineSource,
        params ICleanupTokens[] cleanupTokens
    )
    {
        var source = new J4JCommandLineSource( svcProvider.GetRequiredService<IParser>(),
                                               cleanupTokens );

        builder.Add( source );

        cmdLineSource = source.CommandLineSource;
        options = source.Parser?.Collection;

        return builder;
    }

    public static IConfigurationBuilder AddJ4JCommandLine(
        this IConfigurationBuilder builder,
        IParser parser,
        out CommandLineSource? cmdLineSource,
        params ICleanupTokens[] cleanupTokens
    )
    {
        var source = new J4JCommandLineSource( parser, cleanupTokens );

        builder.Add( source );

        cmdLineSource = source.CommandLineSource;

        return builder;
    }

    public static IConfigurationBuilder AddJ4JCommandLineForWindows(
        this IConfigurationBuilder builder,
        out OptionCollection? options,
        out CommandLineSource? cmdLineSource,
        ITextConverters? converters = null,
        params ICleanupTokens[] cleanupTokens
    ) =>
        builder.AddJ4JCommandLineDefault( CommandLineOperatingSystems.Windows,
                                          out options,
                                          out cmdLineSource,
                                          converters ?? new TextConverters(),
                                          cleanupTokens );

    public static IConfigurationBuilder AddJ4JCommandLineForLinux(
        this IConfigurationBuilder builder,
        out OptionCollection? options,
        out CommandLineSource? cmdLineSource,
        ITextConverters? converters = null,
        params ICleanupTokens[] cleanupTokens
    ) =>
        builder.AddJ4JCommandLineDefault( CommandLineOperatingSystems.Linux,
                                          out options,
                                          out cmdLineSource,
                                          converters ?? new TextConverters(),
                                          cleanupTokens );

    private static IConfigurationBuilder AddJ4JCommandLineDefault(
        this IConfigurationBuilder builder,
        CommandLineOperatingSystems opSys,
        out OptionCollection? options,
        out CommandLineSource? cmdLineSource,
        ITextConverters? converters = null,
        params ICleanupTokens[] cleanupTokens
    )
    {
        converters ??= new TextConverters();

        var textComparison = opSys switch
        {
            CommandLineOperatingSystems.Windows => StringComparison.OrdinalIgnoreCase,
            CommandLineOperatingSystems.Linux => StringComparison.Ordinal,
            _ => throw new
                InvalidEnumArgumentException( $"Unsupported {nameof( CommandLineOperatingSystems )} value '{opSys}'" )
        };

        var lexicalElements = opSys switch
        {
            CommandLineOperatingSystems.Windows => (ILexicalElements) new WindowsLexicalElements(),
            CommandLineOperatingSystems.Linux => new LinuxLexicalElements(),
            _ => throw new
                InvalidEnumArgumentException( $"Unsupported {nameof( CommandLineOperatingSystems )} value '{opSys}'" )
        };

        options = new OptionCollection( textComparison, converters );

        var optionsGenerator = new OptionsGenerator( options, textComparison );
        var parsingTable = new ParsingTable( optionsGenerator );
        var tokenizer = new Tokenizer( lexicalElements );

        return builder.AddJ4JCommandLine( new Parser( options, parsingTable, tokenizer ),
                                          out cmdLineSource,
                                          cleanupTokens );
    }
}
