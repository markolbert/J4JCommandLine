using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.VisualBasic;

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

        public AllocationResult AllocateCommandLine( string[] args, Options options ) =>
            AllocateCommandLine( string.Join( " ", args ), options );

        public AllocationResult AllocateCommandLine( string cmdLine, Options options )
        {
            var retVal = new AllocationResult();
            
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
                    if( options.UsesCommandLineKey( element ) )
                    {
                        curOption = options[element];
                        curOption!.CommandLineKeyProvided = element;

                        lastElementWasKey = true;
                    }
                    else
                    {
                        retVal.UnknownKeys.Add( element );
                        _logger.Error<string>("Unknown key '{0}'", element);
                    }
                }
                else
                {
                    // element is parameter value
                    if( curOption == null || !lastElementWasKey )
                        retVal.UnkeyedParameters.Add( element );
                    else curOption.AddAllocatedValue( element );

                    lastElementWasKey = false;
                }

                // clear the accumulator so we can start processing the next character sequence
                accumulator.Clear();
            }

            return retVal;
        }
    }
}