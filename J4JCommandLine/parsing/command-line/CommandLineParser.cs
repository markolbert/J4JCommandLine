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
            var lastElementWasKey = false;
            var charsProcessed = 0;

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
                    // if the prior element was a keyvalue then it must've
                    // been for a switch (parameterless) option so add a "true"
                    // parameter to it to complete it
                    if( lastElementWasKey )
                        curResult?.Parameters.Add( "true" );

                    lastElementWasKey = true;

                    // because multiple key references are allowed (e.g., "-x abc -x def") check
                    // to see if the key is already recorded. We only create and store a new
                    // ParseResult if the key isn't already stored
                    if( !retVal.Contains( element ) )
                    {
                        var newResult = new ParseResult( retVal ) { Key = element };

                        // if we're adding an new option/key at the end of the command line
                        // it must be a switch (parameterless option), so add "true" to its
                        // parameters
                        if( idx == ( cmdLine.Length - 1 ) )
                            newResult.Parameters.Add( "true" );

                        retVal.Add( newResult );
                    }

                    // store a reference to the current/active ParseResult
                    curResult = retVal[ element ];
                }
                else
                {
                    lastElementWasKey = false;

                    // since the element is an option value store the text as a 
                    // parameter. We store it into curResult because curResult is the
                    // instance associated with the last key value
                    // if curResult is null it's because we haven't encountered our first
                    // key, in which case the parsed text gets assigned to the return values
                    // Unkeyed collection
                    if( curResult == null )
                            retVal.Unkeyed.Parameters.Add( element );
                    else curResult.Parameters.Add( element );
                }

                // clear the accumulator so we can start processing the next character sequence
                accumulator.Clear();
            }

            return retVal;
        }
    }
}