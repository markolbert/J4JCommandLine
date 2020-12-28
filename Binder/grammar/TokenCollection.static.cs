using System;

namespace J4JSoftware.CommandLine
{
    public partial class TokenCollection
    {
        public static TokenCollection GetDefault(
            CommandLineStyle style,
            CommandLineLogger logger,
            StringComparison? textComparison = null )
        {
            textComparison ??= style == CommandLineStyle.Linux
                ? StringComparison.Ordinal
                : StringComparison.OrdinalIgnoreCase;

            var retVal = new TokenCollection( textComparison.Value, logger );

            if( style == CommandLineStyle.UserDefined )
            {
                logger.LogError( "Requested a user-defined default TokenCollection, which contains no tokens" );
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