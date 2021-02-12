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
    public class Parser
    {
        private readonly IJ4JLogger? _logger;
        private readonly ITokenizer _tokenizer;

        public Parser(
            IPropertyValidator? propertyValidator,
            IConverters converters,
            CommandLineStyle style = CommandLineStyle.Windows,
            Func<IJ4JLogger>? loggerFactory = null
        )
            : this( new OptionCollection( style, loggerFactory: loggerFactory ) )
        {
        }

        public Parser( IOptionCollection options )
            : this(
                options,
                new Tokenizer( options.CommandLineStyle, options.MasterText.TextComparison, options.LoggerFactory ),
                new ParsingTable( options, options.LoggerFactory ),
                options.LoggerFactory?.Invoke() )
        {
        }

        public Parser(
            IOptionCollection options,
            ITokenizer tokenizer,
            IParsingTable parsingTable,
            IJ4JLogger? logger = null
        )
        {
            Options = options;
            ParsingTable = parsingTable;
            _tokenizer = tokenizer;
            _logger = logger;
        }

        public IOptionCollection Options { get; }
        public IParsingTable ParsingTable { get; }

        public bool Parse( string cmdLine )
        {
            if( !ParsingTable.IsValid )
            {
                _logger?.Error( "ParsingTable is invalid" );
                return false;
            }

            var prevToken = new Token( TokenType.StartOfInput, string.Empty );

            var allOkay = true;

            foreach( var token in _tokenizer.Tokenize( cmdLine ) )
            {
                allOkay &= ParsingTable[ prevToken.Type, token.Type ]!( prevToken, token, token.Text );

                prevToken = token;
            }

            // always end processing with a commit because there will generally be
            // a pending entry
            allOkay &= ParsingTable.Entries.Commit( prevToken, prevToken, string.Empty );

            return allOkay;
        }
    }
}