﻿using System;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class KeyPrefixer : IElementKey
    {
        private StringComparison _textComp;
        private CommandLineErrors _errors;
        private MasterTextCollection _masterText;

        public bool IsInitialized => _masterText[TextUsageType.Prefix]?.Count() > 0;

        public void Initialize( StringComparison textComp, CommandLineErrors errors, MasterTextCollection masterText )
        {
            _textComp = textComp;
            _errors = errors;
            _masterText = masterText;

            if( !IsInitialized )
                _errors.AddError( null, null, $"No option prefixes were specified" );
        }

        public int GetMaxPrefixLength( string text )
        {
            var retVal = 0;

            if( !IsInitialized )
            {
                _errors.AddError(null, null, $"{nameof(KeyPrefixer)} is not initialized");
                return retVal;
            }

            if ( string.IsNullOrEmpty( text ) )
                return retVal;

            foreach (var prefix in _masterText[TextUsageType.Prefix])
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