using System;
using System.Collections.Generic;
using System.Windows.Input;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Parser : IParser
    {
        private readonly IOptionCollection _options;
        private readonly ICommandLineTextParser _textParser;
        private readonly IParsingConfiguration _config;
        private readonly IJ4JLogger? _logger;

        public Parser(
            IOptionCollection options,
            ICommandLineTextParser textParser,
            IParsingConfiguration config,
            IJ4JLogger? logger = null
        )
        {
            _options = options;
            _textParser = textParser;
            _config = config;
            _logger = logger;

            _logger?.SetLoggedType( this.GetType() );
        }

        public List<IParseResult> Parse( string[] arguments )
        {
            var parsed = _textParser.Parse( arguments );

            foreach( var kvp in parsed )
            {
                var option = _options[ kvp.Key ];

                if( option == null )
                    continue;
            }

            return new List<IParseResult>();
        }
    }
}