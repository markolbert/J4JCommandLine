using System.Linq;
using System.Text;

#pragma warning disable 8618

namespace J4JSoftware.CommandLine
{
    public class Allocator : IAllocator
    {
        private readonly IElementTerminator _terminator;
        private readonly IKeyPrefixer _prefixer;
        private readonly MasterTextCollection _masterText;
        private readonly CommandLineLogger _logger;

        public Allocator( 
            IElementTerminator terminator,
            IKeyPrefixer prefixer,
            MasterTextCollection masterText,
            CommandLineLogger logger
            )
        {
            _terminator = terminator;
            _prefixer = prefixer;
            _masterText = masterText;

            _logger = logger;
        }

        public AllocationResult AllocateCommandLine( string cmdLine, OptionCollection options )
        {
            var retVal = new AllocationResult();
            
            var accumulator = new StringBuilder();
            IOption? curOption = null;
            var charsProcessed = 0;

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
                    }
                    else
                    {
                        retVal.UnknownKeys.Add( element );
                        _logger.LogInformation($"Unknown key '{element}'");
                    }
                }
                else
                {
                    // element is parameter value

                    // strip off any containing quotes
                    var firstQuoteChar = element.Where(c =>
                            _masterText[TextUsageType.Quote].Any(x => x == c.ToString()))
                        .Select(c => c)
                        .FirstOrDefault();

                    if( firstQuoteChar != char.MinValue )
                        element = element.Replace( firstQuoteChar.ToString(), "" );

                    if ( curOption == null 
                        || curOption.NumValuesAllocated >= curOption.MaxValues )
                        retVal.UnkeyedParameters.Add( element );
                    else curOption.AddAllocatedValue( element );
                }

                // clear the accumulator so we can start processing the next character sequence
                accumulator.Clear();
            }

            return retVal;
        }
    }
}