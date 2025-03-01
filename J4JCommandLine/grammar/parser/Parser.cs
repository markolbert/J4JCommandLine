#region copyright

// Copyright (c) 2021, 2022, 2023 Mark A. Olbert 
// https://www.JumpForJoySoftware.com
// Parser.cs
//
// This file is part of JumpForJoy Software's J4JCommandLine.
// 
// J4JCommandLine is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the 
// Free Software Foundation, either version 3 of the License, or 
// (at your option) any later version.
// 
// J4JCommandLine is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
// 
// You should have received a copy of the GNU General Public License along 
// with J4JCommandLine. If not, see <https://www.gnu.org/licenses/>.

#endregion

using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class Parser(
    J4JCommandLineBuilder optionBuilder,
    ParsingTable parsingTable,
    ITokenizer tokenizer
)
    : IParser
{
    public static IParser GetWindowsDefault(
        J4JCommandLineBuilder optionBuilder
    )
    {
        var parsingTable = new ParsingTable( new OptionsGenerator( optionBuilder ) );

        var tokenizer = new Tokenizer( new WindowsLexicalElements( optionBuilder ), optionBuilder );

        return new Parser( optionBuilder, parsingTable, tokenizer );
    }

    public static IParser GetLinuxDefault(
        J4JCommandLineBuilder optionBuilder
    )
    {
        var parsingTable = new ParsingTable( new OptionsGenerator( optionBuilder ) );

        var tokenizer = new Tokenizer( new LinuxLexicalElements( optionBuilder ), optionBuilder );

        return new Parser( optionBuilder, parsingTable, tokenizer );
    }

    private readonly ILogger? _logger = BuildTimeLoggerFactory.Default.Create<Parser>();

    public ITokenizer Tokenizer { get; } = tokenizer;
    public OptionCollection Collection => optionBuilder.Options;

    public bool Parse( string cmdLine )
    {
        var tokenList = Tokenizer.Tokenize( cmdLine );

        foreach( var tokenPair in tokenList.EnumerateTokenPairs() )
        {
            var parsingAction = parsingTable[ tokenPair.LexicalPair ];

            if( parsingAction == null )
            {
                _logger?.UndefinedParsingAction( tokenPair.Previous.Type.ToString(),
                                                 tokenPair.Current.Type.ToString() );

                return false;
            }

            if( !parsingAction( tokenPair ) )
                return false;
        }

        return true;
    }
}
