using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine
{
    public class ElementTerminator : IElementTerminator
    {
        private readonly MasterTextCollection _masterText;
        private readonly CommandLineLogger _logger;

        public ElementTerminator( 
            MasterTextCollection masterText,
            CommandLineLogger logger
            )
        {
            _masterText = masterText;

            _logger = logger;
        }

        public int GetMaxTerminatorLength( string text, bool isKey )
        {
            if( !_masterText.IsValid )
            {
                _logger.Log("MasterTextCollection is not initialized"  );
                throw new TypeInitializationException( "MasterTextCollection is not initialized", null );
            }

            var retVal = 0;

            if( string.IsNullOrEmpty( text ) )
                return retVal;

            // spaces are terminators only if the text contains no paired allowable quotes 
            // or has an even number of allowable quotes (i.e., the quotes are 'closed')
            var closedQuotes = true;

            foreach( var quoteChar in _masterText[TextUsageType.Quote] )
            {
                closedQuotes &= text.Count( c => c == quoteChar[0] ) % 2 == 0;
            }

            if( closedQuotes )
            {
                if( text[ ^1 ] == ' ' )
                    retVal = 1;
            }

            // value enclosers are only terminating characters if we're processing
            // a key
            if( isKey )
            {
                foreach( var terminator in _masterText[TextUsageType.ValueEncloser] )
                {
                    var termLen = terminator.Length;

                    var tail = text.Length >= terminator.Length
                        ? text.Substring( text.Length - termLen, termLen )
                        : string.Empty;

                    if( !string.Equals( terminator, tail, _masterText.TextComparison!.Value ) )
                        continue;

                    if( termLen > retVal )
                        retVal = termLen;
                }
            }

            return retVal;
        }
    }
}