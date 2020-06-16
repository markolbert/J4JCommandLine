using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class CommandLineParser : ICommandLineParser
    {
        private readonly IElementTerminator _terminator;
        private readonly IElementKey _prefixer;

        private StringComparison _keyComp;

        public CommandLineParser( 
            IElementTerminator terminator,
            IElementKey prefixer
            )
        {
            _terminator = terminator;
            _prefixer = prefixer;
        }

        public bool IsInitialized => _prefixer.IsInitialized && _terminator.IsInitialized;

        public bool Initialize( 
            StringComparison keyComp, 
            CommandLineErrors errors,
            MasterTextCollection masterText )
        {
            _keyComp = keyComp;

            _prefixer.Initialize( keyComp, errors, masterText );

            _terminator.Initialize(keyComp, errors, masterText);

            return _prefixer.IsInitialized && _terminator.IsInitialized;
        }

        public ParseResults Parse( string[] args ) => Parse( string.Join( " ", args ) );

        public ParseResults Parse( string cmdLine )
        {
            var retVal = new ParseResults(_keyComp);

            var accumulator = new StringBuilder();
            IParseResult? curResult = null;
            var charsProcessed = 0;
            var lastElementWasKey = false;

            for( var idx = 0; idx < cmdLine.Length; idx++)
            {
                accumulator.Append( cmdLine[idx] );
                charsProcessed++;

                var element = accumulator.ToString();

                // analyze the sequence as it currently stands to see if it includes
                // a prefixed key and/or has been terminated
                var maxPrefix = _prefixer.GetMaxPrefixLength(element);
                var maxTerminator = _terminator.GetMaxTerminatorLength(element, maxPrefix > 0);

                // keep adding characters unless we've encountered a termination 
                // sequence or we've reached the end of the command line
                if( maxTerminator <= 0 && charsProcessed < cmdLine.Length )
                    continue;

                // extract the true element value from the prefix and terminator
                element = element[maxPrefix..^maxTerminator];

                // key values are identified by the presence of known prefixes
                if ( maxPrefix > 0 )
                {
                    // because multiple key references are allowed (e.g., "-x abc -x def") check
                    // to see if the key is already recorded. We only create and store a new
                    // ParseResult if the key isn't already stored
                    if( !retVal.Contains( element ) )
                        retVal.Add( new ParseResult( retVal ) { Key = element } );

                    // store a reference to the current/active ParseResult
                    curResult = retVal[ element ];

                    lastElementWasKey = true;
                }
                else
                {
                    if( curResult == null || !lastElementWasKey )
                            retVal.Unkeyed.Parameters.Add( element );
                    else curResult.Parameters.Add( element );

                    lastElementWasKey = false;
                }

                // clear the accumulator so we can start processing the next character sequence
                accumulator.Clear();
            }

            return retVal;
        }
    }
}