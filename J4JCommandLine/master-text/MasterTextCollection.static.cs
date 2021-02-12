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
            }

            return retVal;
        }
    }
}