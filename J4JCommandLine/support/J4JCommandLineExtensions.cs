#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.Configuration.CommandLine
{
    public static class J4JCommandLineExtensions
    {
        public static StringComparison GetStringComparison( this CommandLineStyle style ) =>
            style switch
            {
                CommandLineStyle.Linux => StringComparison.Ordinal,
                CommandLineStyle.Windows => StringComparison.OrdinalIgnoreCase,
                CommandLineStyle.Universal => StringComparison.OrdinalIgnoreCase,
                _ => throw new InvalidEnumArgumentException( $"Unsupported {nameof(CommandLineStyle)} value '{style}'" )
            };

        public static BindableTypeInfo GetBindableInfo( this Type toCheck )
        {
            if( toCheck.IsArray )
            {
                var elementType = toCheck.GetElementType();

                return elementType == null
                    ? new BindableTypeInfo( toCheck, BindableType.Unsupported )
                    : new BindableTypeInfo( elementType, BindableType.Array );
            }

            // if it's not an array and not a generic it's a "simple" type
            if( !toCheck.IsGenericType )
                return new BindableTypeInfo( toCheck, BindableType.Simple );

            if( toCheck.GenericTypeArguments.Length != 1 )
                return new BindableTypeInfo( toCheck, BindableType.Unsupported );

            var genType = toCheck.GetGenericArguments()[ 0 ];

            return typeof(List<>).MakeGenericType( genType ).IsAssignableFrom( toCheck )
                ? new BindableTypeInfo( genType, BindableType.List )
                : new BindableTypeInfo( toCheck, BindableType.Unsupported );
        }

        #region AddJ4JCommandLine

        // This version works with an explicit CommandLineSource object.
        // It is used by all the other versions
        //
        // All versions requires IJ4JParserFactory and IJ4JLoggerFactory to be
        // registered with the IServiceProvider
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            CommandLineStyle style,
            CommandLineSource cmdLineSource,
            IServiceProvider svcProvider,
            out IParser? parser,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens
        )
        {
            var source = new J4JCommandLineSource(
                style,
                svcProvider.GetRequiredService<IParserFactory>(),
                svcProvider.GetRequiredService<IJ4JLoggerFactory>(),
                cmdLineSource,
                textComparison,
                cleanupTokens);

            builder.Add(source);

            parser = source.Parser;

            return builder;
        }

        // This version parses an explicitly-provided command line 
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            CommandLineStyle style,
            string cmdLine,
            IServiceProvider svcProvider,
            out IParser? parser,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens
        ) => builder.AddJ4JCommandLine(
            style,
            new CommandLineSource( cmdLine ),
            svcProvider,
            out parser,
            textComparison,
            cleanupTokens );

        // This version retrieves the command line via RawCommandLine.GetRawCommandLine(),
        // which works behind the scenes to grab the command line provided to the app
        // that's running
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            CommandLineStyle style,
            IServiceProvider svcProvider,
            out IParser? parser,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens
        ) => builder.AddJ4JCommandLine(
            style,
            new CommandLineSource( new RawCommandLine().GetRawCommandLine() ),
            svcProvider,
            out parser,
            textComparison,
            cleanupTokens );

        // This version parses an explicitly-provided string[] of arguments
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            CommandLineStyle style,
            string[] args,
            IServiceProvider svcProvider,
            out IParser? parser,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens
        ) => builder.AddJ4JCommandLine(
            style,
            new CommandLineSource(args),
            svcProvider,
            out parser,
            textComparison,
            cleanupTokens);

        // This version parses an explicitly-provided IEnumerable<string> of arguments
        public static IConfigurationBuilder AddJ4JCommandLine(
            this IConfigurationBuilder builder,
            CommandLineStyle style,
            IEnumerable<string> args,
            IServiceProvider svcProvider,
            out IParser? parser,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens
        ) => builder.AddJ4JCommandLine(
            style,
            new CommandLineSource(args),
            svcProvider,
            out parser,
            textComparison,
            cleanupTokens);

        #endregion
    }
}