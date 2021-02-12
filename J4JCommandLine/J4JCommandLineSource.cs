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

using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine
{
    public class J4JCommandLineSource : IConfigurationSource
    {
        private readonly string _cmdLine;
        private readonly Parser _parser;

        public J4JCommandLineSource(
            IOptionCollection options,
            string cmdLine
        )
        {
            Options = options;
            _cmdLine = cmdLine;

            var tokenizer = new Tokenizer( options.CommandLineStyle, options.MasterText.TextComparison,
                options.LoggerFactory );
            var parsingTable = new ParsingTable( options, options.LoggerFactory );

            _parser = new Parser( options, tokenizer, parsingTable, options.LoggerFactory?.Invoke() );
        }

        public J4JCommandLineSource(
            IOptionCollection options,
            string[] args
        )
            : this( options, string.Join( " ", args ) )
        {
        }

        public IOptionCollection Options { get; }

        public IConfigurationProvider Build( IConfigurationBuilder builder )
        {
            return new J4JCommandLineProvider( this );
        }

        public bool Parse()
        {
            return _parser.Parse( _cmdLine );
        }
    }
}