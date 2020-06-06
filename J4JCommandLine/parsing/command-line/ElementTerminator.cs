using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class ElementTerminator : IElementTerminator
    {
        private readonly List<char> _quotes = new List<char>();

        private StringComparison _textComp;

        public bool IsInitialized => ValueEnclosers != null;

        public UniqueText ValueEnclosers { get; private set; }
        public ReadOnlyCollection<char> QuoteCharacters => _quotes.AsReadOnly();

        public void Initialize( 
            StringComparison textComp,
            IEnumerable<string>? enclosers = null,
            IEnumerable<char>? quoteChars = null)
        {
            _textComp = textComp;

            if( quoteChars != null )
                _quotes.AddRange( quoteChars );

            ValueEnclosers = new UniqueText(_textComp);

            if( enclosers != null )
                ValueEnclosers.AddRange( enclosers );
        }

        public int GetMaxTerminatorLength( string text, bool isKey )
        {
            if( !IsInitialized )
                throw new ApplicationException( $"{this.GetType()} is not initialized" );
            var retVal = 0;

            if( string.IsNullOrEmpty( text ) )
                return retVal;

            // spaces are terminators only if the text contains no paired allowable quotes 
            // or has an even number of allowable quotes (i.e., the quotes are 'closed')
            var closedQuotes = true;

            foreach( var quoteChar in _quotes )
            {
                closedQuotes &= text.Count( c => c == quoteChar ) % 2 == 0;
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
                foreach( var terminator in ValueEnclosers )
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