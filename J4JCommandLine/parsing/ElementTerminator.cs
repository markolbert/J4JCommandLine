using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class ElementTerminator : IElementTerminator
    {
        private readonly IParsingConfiguration _parsingConfig;

        public ElementTerminator( IParsingConfiguration parsingConfig )
        {
            _parsingConfig = parsingConfig;
        }

        public int GetMaxTerminatorLength( ElementProcessor processor )
        {
            var retVal = 0;

            if( string.IsNullOrEmpty( processor.Text ) )
                return retVal;

            // spaces are terminators only if the text contains no single or double quotes 
            // or has an even number of quotes or double quotes (i.e., the quotes are 'closed')
            var numSingleQuotes = processor.Text.Count( c => c == '"' ) % 2;
            var numDoubleQuotes = processor.Text.Count(c => c == '\'') % 2;

            if( numDoubleQuotes == 0 && numSingleQuotes == 0 )
            {
                if (processor.Text[^1] == ' ')
                    retVal = 1;
            }

            // value enclosers are only terminating characters if we're processing
            // a key
            if ( processor.ElementType == ElementType.Key )
            {
                foreach( var terminator in _parsingConfig.ValueEnclosers )
                {
                    var termLen = terminator.Length;

                    var tail = processor.Text.Length >= terminator.Length
                        ? processor.Text.Substring( processor.Text.Length - termLen, termLen )
                        : string.Empty;

                    if( !string.Equals( terminator, tail, _parsingConfig.TextComparison ) )
                        continue;

                    if( termLen > retVal )
                        retVal = termLen;
                }
            }

            return retVal;
        }
    }
}