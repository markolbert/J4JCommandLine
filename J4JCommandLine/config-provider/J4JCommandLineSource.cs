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
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine
{
    public class J4JCommandLineSource : IConfigurationSource
    {
        public event EventHandler? SourceChanged;

        private readonly IJ4JLogger? _logger;

        public J4JCommandLineSource(
            IParser parser,
            IJ4JLogger? logger,
            params ICleanupTokens[] cleanupTokens
        )
        {
            Parser = parser;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            CommandLineSource = Initialize();
        }

        public J4JCommandLineSource(
            IParser parser,
            IJ4JLogger? logger
        )
        {
            Parser = parser;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );

            CommandLineSource = Initialize();
        }

        private CommandLineSource? Initialize()
        {
            if( Parser == null )
                return null;

            Parser.Options.Configured += Options_Configured;

            var retVal = new CommandLineSource( Parser.Tokenizer.TextComparison );
            retVal.Changed += OnCommandLineSourceChanged;

            return retVal;
        }

        private void Options_Configured(object? sender, EventArgs e) =>
            SourceChanged?.Invoke(this, EventArgs.Empty);

        private void OnCommandLineSourceChanged( object? sender, ConfigurationChangedEventArgs e ) =>
            SourceChanged?.Invoke(this, EventArgs.Empty);

        public CommandLineSource? CommandLineSource { get; }
        public IParser? Parser { get; }

        public IConfigurationProvider Build( IConfigurationBuilder builder )
        {
            return new J4JCommandLineProvider( this );
        }
    }
}