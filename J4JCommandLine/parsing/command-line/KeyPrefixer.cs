using System;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class KeyPrefixer : IElementKey
    {
        private StringComparison _textComp;

        public bool IsInitialized => Prefixes?.Count() > 0;

        public UniqueText Prefixes { get; private set; }

        public void Initialize( StringComparison textComp, params string[] prefixers )
        {
            _textComp = textComp;

            Prefixes = new UniqueText( _textComp );
            Prefixes.AddRange( prefixers );
        }

        public int GetMaxPrefixLength( string text )
        {
            if( !IsInitialized )
                throw new ApplicationException( $"{this.GetType()} is is not initialized" );

            var retVal = 0;

            if( string.IsNullOrEmpty( text ) )
                return retVal;

            foreach (var prefix in Prefixes)
            {
                var prefixLen = prefix.Length;

                var start = text.Length >= prefix.Length
                    ? text.Substring(0, prefixLen)
                    : string.Empty;

                if (!string.Equals(prefix, start, _textComp))
                    continue;

                if (prefixLen > retVal)
                    retVal = prefixLen;
            }

            // the prefix must be followed by an alphabetic character for the text to be
            // a key
            var hasAlpha = text.Length >= retVal + 1 && char.IsLetter( text[ retVal ] );

            return retVal > 0 && hasAlpha ? retVal : 0;
        }
    }
}