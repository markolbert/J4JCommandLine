using System;
using System.ComponentModel;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public partial class MasterTextCollection
    {
        public static MasterTextCollection GetDefault(
            CommandLineStyle cmdLineStyle,
            Func<IJ4JLogger>? loggerFactory,
            StringComparison? comparison = null )
        {
            var retVal = new MasterTextCollection( comparison ?? StringComparison.OrdinalIgnoreCase, loggerFactory );

            switch( cmdLineStyle )
            {
                case CommandLineStyle.Linux:
                    retVal.AddRange( TextUsageType.Prefix, "-", "--" );
                    retVal.AddRange( TextUsageType.Quote, "\"", "'" );
                    retVal.Add( TextUsageType.ValueEncloser, "=" );
                    retVal.AddRange( TextUsageType.Separator, " ", "\t" );

                    return retVal;

                case CommandLineStyle.Universal:
                    retVal.AddRange( TextUsageType.Prefix, "-", "--", "/" );
                    retVal.AddRange( TextUsageType.Quote, "\"", "'" );
                    retVal.Add( TextUsageType.ValueEncloser, "=" );
                    retVal.AddRange( TextUsageType.Separator, " ", "\t" );

                    return retVal;

                case CommandLineStyle.Windows:
                    retVal.AddRange( TextUsageType.Prefix, "/", "-", "--" );
                    retVal.Add( TextUsageType.Quote, "\"" );
                    retVal.Add( TextUsageType.ValueEncloser, "=" );
                    retVal.AddRange( TextUsageType.Separator, " ", "\t" );

                    return retVal;

                default:
                    throw new InvalidEnumArgumentException( $"Unsupported CommandLineStyle '{cmdLineStyle}'" );
            }
        }
    }
}