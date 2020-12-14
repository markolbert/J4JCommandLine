using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using J4JSoftware.Logging;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine
{
    public class Allocator : IAllocator
    {
        private readonly IElementTerminator _terminator;
        private readonly IKeyPrefixer _prefixer;
        private readonly IJ4JLogger _logger;

        public Allocator( 
            IElementTerminator terminator,
            IKeyPrefixer prefixer,
            IJ4JLogger logger
            )
        {
            _terminator = terminator;
            _prefixer = prefixer;

            _logger = logger;
            _logger.SetLoggedType( GetType() );
        }

        public bool AllocateCommandLine( string[] args, Options options, out List<string>? unkeyed )
        {
            unkeyed = null;

            if( !AllocateCommandLine( string.Join( " ", args ), options, out var temp ) )
                return false;

            unkeyed = temp;

            return true;
        }

        public bool AllocateCommandLine( string cmdLine, Options options, out List<string>? unkeyed )
        {
            unkeyed = null;
            var unkeyedInternal = new List<string>();

            var accumulator = new StringBuilder();
            Option? curOption = null;
            var charsProcessed = 0;
            var lastElementWasKey = false;

            for( var idx = 0; idx < cmdLine.Length; idx++ )
            {
                accumulator.Append( cmdLine[ idx ] );
                charsProcessed++;

                var element = accumulator.ToString();

                // analyze the sequence as it currently stands to see if it includes
                // a prefixed key and/or has been terminated
                var maxPrefix = _prefixer.GetMaxPrefixLength( element );
                var maxTerminator = _terminator.GetMaxTerminatorLength( element, maxPrefix > 0 );

                // keep adding characters unless we've encountered a termination 
                // sequence or we've reached the end of the command line
                if( maxTerminator <= 0 && charsProcessed < cmdLine.Length )
                    continue;

                // extract the true element value from the prefix and terminator
                element = element[ maxPrefix..^maxTerminator ];

                // key values are identified by the presence of known prefixes
                if( maxPrefix > 0 )
                {
                    // element is a key

                    // if the key (contained in element) isn't among the keys defined
                    // in the options collection we have a problem
                    if( !options.UsesCommandLineKey( element ) )
                    {
                        _logger.Error<string>( "Unknown key '{0}'", element );
                        return false;
                    }

                    curOption = options[ element ];

                    curOption!.CommandLineKeyUsed = element;

                    lastElementWasKey = true;
                }
                else
                {
                    // element is parameter value
                    if( curOption == null || !lastElementWasKey )
                        unkeyedInternal.Add( element );
                    else curOption.AddAllocatedValue( element );

                    lastElementWasKey = false;
                }

                // clear the accumulator so we can start processing the next character sequence
                accumulator.Clear();
            }

            unkeyed = unkeyedInternal;

            return true;
        }
    }
}