using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class ElementTerminator : IElementTerminator
    {
        private StringComparison _textComp;
        private CommandLineLogger _logger;
        private MasterTextCollection _masterText;

        public bool IsInitialized {get; private set; }

        public void Initialize( 
            StringComparison textComp,
            CommandLineLogger logger,
            MasterTextCollection masterText )
        {
            _textComp = textComp;
            _logger = logger;
            _masterText = masterText;

            IsInitialized = true;
        }

        public int GetMaxTerminatorLength( string text, bool isKey )
        {
            var retVal = 0;

            if( !IsInitialized )
            {
                _logger.LogError( ProcessingPhase.Allocating, $"{nameof(ElementTerminator)} is not initialized" );
                return retVal;
            }

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

                    if( !string.Equals( terminator, tail, _textComp ) )
                        continue;

                    if( termLen > retVal )
                        retVal = termLen;
                }
            }

            return retVal;
        }
    }
}