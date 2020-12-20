using System;
using System.Linq;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine
{
    public class KeyPrefixer : IKeyPrefixer
    {
        private readonly MasterTextCollection _masterText;
        private readonly CommandLineLogger _logger;

        public KeyPrefixer(
            MasterTextCollection masterText,
            CommandLineLogger logger
        )
        {
            _masterText = masterText;

            _logger = logger;

            if( _masterText[ TextUsageType.Prefix ].Any() ) return;

            var mesg =
                $"{nameof(masterText)} ({typeof(MasterTextCollection)}) does not define any {nameof(TextUsageType.Prefix)} entries";
            _logger.Log( mesg );

            throw new ArgumentException( mesg );
        }

        public int GetMaxPrefixLength( string text )
        {
            if (!_masterText.IsValid)
            {
                _logger.Log("MasterTextCollection is not initialized");
                throw new TypeInitializationException("MasterTextCollection is not initialized", null);
            }

            var retVal = 0;

            if ( string.IsNullOrEmpty( text ) )
                return retVal;

            foreach (var prefix in _masterText[TextUsageType.Prefix])
            {
                var prefixLen = prefix.Length;

                var start = text.Length >= prefix.Length
                    ? text.Substring(0, prefixLen)
                    : string.Empty;

                if (!string.Equals(prefix, start, _masterText.TextComparison!.Value))
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