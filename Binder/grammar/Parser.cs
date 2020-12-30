namespace J4JSoftware.Configuration.CommandLine
{
    public class Parser
    {
        private readonly CommandLineLogger _logger;
        private readonly ITokenizer _tokenizer;

        public Parser(
            CommandLineLogger logger,
            CommandLineStyle style = CommandLineStyle.Windows
        )
            : this( new OptionCollectionNG( style ), logger )
        {
        }

        public Parser(
            IOptionCollection options,
            CommandLineLogger logger
        )
            : this(
                options,
                new Tokenizer( logger, options.CommandLineStyle, options.MasterText.TextComparison ),
                new ParsingTable( options, logger ),
                logger )
        {
        }

        public Parser(
            IOptionCollection options,
            ITokenizer tokenizer,
            IParsingTable parsingTable,
            CommandLineLogger logger
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
                _logger.LogError( "ParsingTable is invalid" );
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