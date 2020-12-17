using System.ComponentModel;
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
                switch( option.Style )
                {
                    case OptionStyle.Switch:
                        Set( option.ContextPath,
                            string.IsNullOrEmpty( option.CommandLineKeyProvided )
                                ? "false"
                                : "true" );
                        
                        break;

                    case OptionStyle.SingleValued:
                        if( option.NumValuesAllocated > 0)
                            Set(option.ContextPath, option.CommandLineValues[0]);

                        break;

                    case OptionStyle.ConcatenatedSingleValue:
                        // concatenated single value properties (e.g., flag enums) are
                        // single valued from a target point of view (i.e., they're not
                        // collections), but they contain multiple string values from
                        // allocating the command line
                        if (option.NumValuesAllocated > 0)
                                Set( option.ContextPath, string.Join( ", ", option.CommandLineValues ) );

                        break;

                    case OptionStyle.Collection:
                        for( var idx = 0; idx < option.NumValuesAllocated; idx++ )
                        {
                            Set( $"{option.ContextPath}:{idx}", option.CommandLineValues[ idx ] );
                        }

                        break;

                    default:
                        throw new InvalidEnumArgumentException( $"Unsupported OptionStyle '{option.Style}'" );
                }
            }
        }
    }
}