using System;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;

namespace J4JSoftware.Configuration.CommandLine
{
    public class J4JCommandLineSource : IConfigurationSource
    {
        private readonly string _cmdLine;
        private readonly Parser _parser;

        public J4JCommandLineSource(
            IOptionCollection options,
            string cmdLine,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            Options = options;
            _cmdLine = cmdLine;

            var tokenizer = new Tokenizer( options.CommandLineStyle, options.MasterText.TextComparison, loggerFactory );
            var parsingTable = new ParsingTable( options, loggerFactory );

            _parser = new Parser( options, tokenizer, parsingTable, loggerFactory?.Invoke() );
        }

        public J4JCommandLineSource(
            IOptionCollection options,
            string[] args,
            Func<IJ4JLogger>? loggerFactory
        )
            : this( options, string.Join( " ", args ), loggerFactory )
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