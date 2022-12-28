// Copyright (c) 2021, 2022 Mark A. Olbert 
// 
// This file is part of J4JCommandLine.
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

using System;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class Parser : IParser
{
    public static IParser GetWindowsDefault( ITextConverters? converters = null,
        IJ4JLogger? logger = null,
        params ICleanupTokens[] cleanupProcessors )
    {
        converters ??= new TextConverters( logger: logger );

        var options = new OptionCollection( StringComparison.OrdinalIgnoreCase, converters, logger );

        var parsingTable = new ParsingTable( new OptionsGenerator( options,
                                                                   StringComparison.OrdinalIgnoreCase,
                                                                   logger ),
                                             logger );

        var tokenizer = new Tokenizer( new WindowsLexicalElements( logger ),
                                       logger,
                                       cleanupProcessors );

        return new Parser( options, parsingTable, tokenizer, logger );
    }

    public static IParser GetLinuxDefault( ITextConverters? converters = null,
        IJ4JLogger? logger = null,
        params ICleanupTokens[] cleanupProcessors )
    {
        converters ??= new TextConverters( logger: logger );

        var options = new OptionCollection( StringComparison.Ordinal, converters, logger );

        var parsingTable = new ParsingTable( new OptionsGenerator( options,
                                                                   StringComparison.Ordinal,
                                                                   logger ),
                                             logger );

        var tokenizer = new Tokenizer( new LinuxLexicalElements( logger ),
                                       logger,
                                       cleanupProcessors );

        return new Parser( options, parsingTable, tokenizer, logger );
    }

    private readonly ParsingTable _parsingTable;
    private readonly IJ4JLogger? _logger;

    public Parser( OptionCollection options,
        ParsingTable parsingTable,
        ITokenizer tokenizer,
        IJ4JLogger? logger = null )
    {
        Collection = options;
        _parsingTable = parsingTable;
        Tokenizer = tokenizer;

        _logger = logger;
    }

    public ITokenizer Tokenizer { get; }
    public OptionCollection Collection { get; }

    public bool Parse( string cmdLine )
    {
        var tokenList = Tokenizer!.Tokenize( cmdLine );

        foreach( var tokenPair in tokenList.EnumerateTokenPairs() )
        {
            var parsingAction = _parsingTable[ tokenPair.LexicalPair ];

            if( parsingAction == null )
            {
                _logger?.Error( "Undefined parsing action for token sequence '{0} => {1}'",
                                tokenPair.Previous.Type,
                                tokenPair.Current.Type );

                return false;
            }

            if( !parsingAction( tokenPair ) )
                return false;
        }

        return true;
    }
}