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
            Logger = new CommandLineLogger();

            var tokenizer = new Tokenizer( Logger, options.CommandLineStyle, options.MasterText.TextComparison );
            var parsingTable = new ParsingTable( options, Logger );

            _parser = new Parser( options, tokenizer, parsingTable, Logger );
        }

        public J4JCommandLineSource(
            IOptionCollection options,
            string[] args,
            CommandLineLogger logger
        )
            : this( options, string.Join( " ", args ) )
        {
        }

        public CommandLineLogger Logger { get; }
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