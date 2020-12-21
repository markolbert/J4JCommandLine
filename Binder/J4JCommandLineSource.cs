using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public class J4JCommandLineSource : IConfigurationSource
    {
        public J4JCommandLineSource( 
            OptionCollection options, 
            string cmdLine 
            )
        {
            Options = options;
            CommandLine = cmdLine;
        }

        public J4JCommandLineSource(
            OptionCollection options,
            string[] args
        )
            : this( options, string.Join( " ", args ) )
        {
        }

        public OptionCollection Options { get; }
        public string CommandLine { get; }

        public IConfigurationProvider Build( IConfigurationBuilder builder )
        {
            return new J4JCommandLineProvider( this );
        }
    }
}
