﻿using System;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class AvailableTokens
    {
        public static AvailableTokens GetDefault(
            CommandLineStyle style,
            Func<IJ4JLogger>? loggerFactory,
            StringComparison? textComparison = null )
        {
            textComparison ??= style == CommandLineStyle.Linux
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            var retVal = new AvailableTokens( textComparison.Value, loggerFactory?.Invoke() );

            var logger = loggerFactory?.Invoke();

            if( style == CommandLineStyle.UserDefined )
            {
                logger?.Error( "Requested a user-defined default AvailableTokens, which contains no tokens" );
                return retVal;
            }

            retVal.Add( TokenType.Separator, " " );
            retVal.Add( TokenType.Separator, "\t" );
            retVal.Add( TokenType.ValuePrefix, "=" );

            switch( style )
            {
                case CommandLineStyle.Linux:
                    retVal.Add( TokenType.Quoter, "\"" );
                    retVal.Add( TokenType.Quoter, "'" );
                    retVal.Add( TokenType.KeyPrefix, "-" );
                    retVal.Add( TokenType.KeyPrefix, "--" );

                    break;

                case CommandLineStyle.Windows:
                    retVal.Add( TokenType.Quoter, "\"" );
                    retVal.Add( TokenType.Quoter, "'" );
                    retVal.Add( TokenType.KeyPrefix, "/" );

                    break;

                case CommandLineStyle.Universal:
                    retVal.Add( TokenType.Quoter, "\"" );
                    retVal.Add( TokenType.Quoter, "'" );
                    retVal.Add( TokenType.KeyPrefix, "-" );
                    retVal.Add( TokenType.KeyPrefix, "--" );
                    retVal.Add( TokenType.KeyPrefix, "/" );

                    break;
            }

            return retVal;
        }
    }
}