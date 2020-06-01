namespace J4JSoftware.CommandLine
{
    public class KeyPrefixer : IElementKey
    {
        private readonly IParsingConfiguration _parsingConfig;

        public KeyPrefixer(IParsingConfiguration parsingConfig)
        {
            _parsingConfig = parsingConfig;
        }

        public int GetMaxPrefixLength( ElementProcessor processor )
        {
            var retVal = 0;

            if( string.IsNullOrEmpty( processor.Text ) )
                return retVal;

            foreach (var prefix in _parsingConfig.Prefixes)
            {
                var prefixLen = prefix.Length;

                var start = processor.Text.Length >= prefix.Length
                    ? processor.Text.Substring(0, prefixLen)
                    : string.Empty;

                if (!string.Equals(prefix, start, _parsingConfig.TextComparison))
                    continue;

                if (prefixLen > retVal)
                    retVal = prefixLen;
            }

            // the prefix must be followed by an alphabetic character for the text to be
            // a key
            var hasAlpha = processor.Text.Length >= retVal + 1 && char.IsLetter( processor.Text[ retVal ] );

            return retVal > 0 && hasAlpha ? retVal : 0;
        }
    }
}