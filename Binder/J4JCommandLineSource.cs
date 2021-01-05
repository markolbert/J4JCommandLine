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

            var tokenizer = new Tokenizer( options.CommandLineStyle, options.MasterText.TextComparison, options.LoggerFactory );
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