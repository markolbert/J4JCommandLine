namespace J4JSoftware.CommandLine
{
    public class KeyPrefixer : IElementKey
    {
        private readonly IParsingConfiguration _parsingConfig;

        public KeyPrefixer(IParsingConfiguration parsingConfig)
        {
            _parsingConfig = parsingConfig;
        }

        public int GetMaxPrefixLength( string text )
        {
            var retVal = 0;

            if( string.IsNullOrEmpty( text ) )
                return retVal;

            foreach (var prefix in _parsingConfig.Prefixes)
            {
                var prefixLen = prefix.Length;

                var start = text.Length >= prefix.Length
                    ? text.Substring(0, prefixLen)
                    : string.Empty;

                if (!string.Equals(prefix, start, _parsingConfig.TextComparison))
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