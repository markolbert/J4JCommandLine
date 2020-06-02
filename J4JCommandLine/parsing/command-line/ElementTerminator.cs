using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class ElementTerminator : IElementTerminator
    {
        private readonly IParsingConfiguration _parsingConfig;

        public ElementTerminator( IParsingConfiguration parsingConfig )
        {
            _parsingConfig = parsingConfig;
        }

        public int GetMaxTerminatorLength( string text, bool isKey )
        {
            var retVal = 0;

            if( string.IsNullOrEmpty( text ) )
                return retVal;

            // spaces are terminators only if the text contains no single or double quotes 
            // or has an even number of quotes or double quotes (i.e., the quotes are 'closed')
            var numSingleQuotes = text.Count( c => c == '"' ) % 2;
            var numDoubleQuotes = text.Count( c => c == '\'' ) % 2;

            if( numDoubleQuotes == 0 && numSingleQuotes == 0 )
            {
                if( text[ ^1 ] == ' ' )
                    retVal = 1;
            }

            // value enclosers are only terminating characters if we're processing
            // a key
            if( isKey )
            {
                foreach( var terminator in _parsingConfig.ValueEnclosers )
                {
                    var termLen = terminator.Length;

                    var tail = text.Length >= terminator.Length
                        ? text.Substring( text.Length - termLen, termLen )
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