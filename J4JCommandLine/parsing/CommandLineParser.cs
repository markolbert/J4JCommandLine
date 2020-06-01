namespace J4JSoftware.CommandLine
{
    public class CommandLineParser : ICommandLineTextParser
    {
        private readonly ParseResults _results;

        private readonly ElementProcessor _processor;
        private bool _foundFirstKey = false;

        public CommandLineParser( 
            ElementProcessor processor,
            IParsingConfiguration parsingConfig
            )
        {
            _processor = processor;
            _results = new ParseResults( parsingConfig );

            _processor.StoreEvent += ProcessorStoreEvent;
        }

        public ParseResults Parse( string[] args ) => Parse( string.Join( " ", args ) );

        public ParseResults Parse( string cmdLine )
        {
            _results.Clear();
            _processor.Clear();

            _foundFirstKey = false;

            foreach (var curChar in cmdLine)
            {
                ProcessCharacter(_processor, curChar);
            }

            _processor.ProcessPendingText(true);

            return _results;
        }

        private void ProcessCharacter( ElementProcessor processor, char curChar )
        {
            processor.AddCharacter(curChar);

            // if a termination sequence hasn't appeared there's nothing else to do
            if( processor.MaxTerminatorLength <= 0 ) 
                return;

            if( !_foundFirstKey )
            {
                if( processor.ElementType != ElementType.Key )
                {
                    // PlainText elements are discarded before the first
                    // key appears
                    processor.Clear();

                    return;
                }

                _foundFirstKey = true;
            }

            // whether the element is a key or plain text, it's fully-defined,
            // so process it (the processing is different internally)
            processor.ProcessPendingText();
        }

        private void ProcessorStoreEvent(object? sender, ParseResult e)
        {
            // because duplicate keys (e.g., "-x 1 -x 2") are allowed we need to check
            // for existing keys and merge the results 
            if( _results.Contains(e.Key))
                _results[e.Key].Parameters.AddRange(e.Parameters);
            else _results.Add(e);
        }
    }
}