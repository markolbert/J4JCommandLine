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

namespace J4JSoftware.Configuration.CommandLine
{
    public class ParserConfiguration
    {
        private IOptionCollection? _options;
        private IAvailableTokens? _tokens;

        public ParserConfiguration(CommandLineStyle style, StringComparison textComparison)
        {
            Style = style;
            TextComparison = textComparison;
        }

        public bool IsValid => Options != null && Tokens != null;

        public CommandLineStyle Style { get; }
        public StringComparison TextComparison { get; }

        public IOptionCollection? Options
        {
            get => _options;
            set => _options = ( value == null || value.Style != Style ) ? null : value;
        }

        public IAvailableTokens? Tokens
        {
            get => _tokens;
            set => _tokens = (value == null || value.Style != Style) ? null : value;
        }
    }
}