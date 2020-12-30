using System;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class Parser
    {
        private readonly IJ4JLogger? _logger;
        private readonly ITokenizer _tokenizer;

        public Parser(
            CommandLineStyle style = CommandLineStyle.Windows,
            Func<IJ4JLogger>? loggerFactory = null
        )
            : this( new OptionCollectionNG( style ), loggerFactory )
        {
        }

        public Parser(
            IOptionCollection options,
            Func<IJ4JLogger>? loggerFactory = null
        )
            : this(
                options,
                new Tokenizer( options.CommandLineStyle, options.MasterText.TextComparison, loggerFactory ),
                new ParsingTable( options, loggerFactory ),
                loggerFactory?.Invoke() )
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

            var tokens = _tokenizer.Tokenize( cmdLine );
            foreach( var token in tokens )
                //foreach( var token in _tokenizer.Tokenize( cmdLine ) )
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