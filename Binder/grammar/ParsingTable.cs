using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class ParsingTable : IParsingTable
    {
        public delegate bool ParsingAction( Token prevToken, Token curToken, params string[] arguments );

        private readonly Dictionary<TokenType, Dictionary<TokenType, ParsingAction?>> _table =
            new();

        private readonly IJ4JLogger? _logger;

        public ParsingTable( IOptionCollection options, Func<IJ4JLogger>? loggerFactory = null )
        {
            _logger = loggerFactory?.Invoke();

            Entries = new TokenEntry.TokenEntries( options, loggerFactory?.Invoke() );

            foreach( var row in Enum.GetValues( typeof(TokenType) )
                .Cast<TokenType>() )
            {
                // Quoter never appears in the list of processed tokens because
                // they are eliminated by the token cleanup routine
                if( row == TokenType.Quoter )
                    continue;

                _table.Add( row, new Dictionary<TokenType, ParsingAction?>() );

                foreach( var col in Enum.GetValues( typeof(TokenType) )
                        .Cast<TokenType>() )
                    // StartOfInput is only a row entry, not a column entry,
                    // because it can never appear as a "next" token

                    // Quoter never appears in the list of processed tokens because
                    // they are eliminated by the token cleanup routine
                    if( col != TokenType.StartOfInput && col != TokenType.Quoter )
                        _table[ row ].Add( col, null );
            }

            this[ TokenType.StartOfInput, TokenType.KeyPrefix ] = Entries.Create;
            this[ TokenType.StartOfInput, TokenType.ValuePrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.StartOfInput, TokenType.Separator ] = Entries.ConsumeToken;
            this[ TokenType.StartOfInput, TokenType.Text ] = Entries.ProcessText;

            this[ TokenType.KeyPrefix, TokenType.KeyPrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.KeyPrefix, TokenType.ValuePrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.KeyPrefix, TokenType.Separator ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.KeyPrefix, TokenType.Text ] = Entries.ProcessText;

            this[ TokenType.ValuePrefix, TokenType.KeyPrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.ValuePrefix, TokenType.ValuePrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.ValuePrefix, TokenType.Separator ] = Entries.ConsumeToken;
            this[ TokenType.ValuePrefix, TokenType.Text ] = Entries.ProcessText;

            this[ TokenType.Separator, TokenType.KeyPrefix ] = Entries.Commit;
            this[ TokenType.Separator, TokenType.ValuePrefix ] = Entries.ConsumeToken;
            this[ TokenType.Separator, TokenType.Separator ] = Entries.ConsumeToken;
            this[ TokenType.Separator, TokenType.Text ] = Entries.ProcessText;

            this[ TokenType.Text, TokenType.KeyPrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.Text, TokenType.ValuePrefix ] = Entries.TerminateWithPrejuidice;
            this[ TokenType.Text, TokenType.Separator ] = Entries.ConsumeToken;
            this[ TokenType.Text, TokenType.Text ] = Entries.ProcessText;
        }

        public bool IsValid => !_table
            .Any( row =>
                row.Value.Any( col =>
                    col.Value == null ) );

        public TokenEntry.TokenEntries Entries { get; }

        public ParsingAction? this[ TokenType row, TokenType col ]
        {
            get => _table[ row ][ col ];
            set => _table[ row ][ col ] = value;
        }
    }
}