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
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine
{
    public class J4JCommandLineSource : IConfigurationSource
    {
        private readonly CommandLineSource _cmdLineSource;
        private readonly IJ4JLogger? _logger;

        public J4JCommandLineSource(
            CommandLineStyle style,
            IParserFactory parserFactory,
            IJ4JLoggerFactory? loggerFactory,
            CommandLineSource cmdLineSource,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens
        )
        {
            _cmdLineSource = cmdLineSource;

            _logger = loggerFactory?.CreateLogger<J4JCommandLineSource>();

            if( !parserFactory.Create( style, out var temp, textComparison, cleanupTokens ) )
                _logger?.Fatal("Could not create instance of IParser");

            Parser = temp;
        }

        public IParser? Parser { get; }

        public bool Parse() => Parser?.Parse( _cmdLineSource.CommandLine ) ?? false;
        
        public IConfigurationProvider Build( IConfigurationBuilder builder )
        {
            return new J4JCommandLineProvider( this );
        }
    }
}