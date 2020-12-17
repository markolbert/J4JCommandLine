using Microsoft.Extensions.Configuration;

namespace J4JSoftware.CommandLine
{
    public class J4JCommandLineProvider : ConfigurationProvider
    {
        public J4JCommandLineProvider( J4JCommandLineSource source )
        {
            Source = source;
        }

        public J4JCommandLineSource Source { get; }

        public override void Load()
        {
            Source.Allocator.AllocateCommandLine( Source.CommandLine, Source.Options );
            
            foreach( var option in Source.Options )
            {
                Set( option.ContextPath, string.Join( ",", option.CommandLineValues ) );
            }
        }
    }
}