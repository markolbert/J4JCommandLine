using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Twilio.Rest.Api.V2010.Account.Sip.Domain.AuthTypes.AuthTypeCalls;

namespace J4JSoftware.CommandLine
{
    public class J4JCommandLineSource : IConfigurationSource
    {
        public J4JCommandLineSource( 
            OptionCollection options, 
            string cmdLine, 
            IAllocator allocator
            )
        {
            Options = options;
            CommandLine = cmdLine;
            Allocator = allocator;
        }

        public J4JCommandLineSource(
            OptionCollection options,
            string[] args,
            IAllocator allocator
        )
            : this( options, string.Join( " ", args ), allocator )
        {
        }

        public OptionCollection Options { get; }
        public string CommandLine { get; }
        public IAllocator Allocator { get; }

        public IConfigurationProvider Build( IConfigurationBuilder builder )
        {
            return new J4JCommandLineProvider( this );
        }
    }
}
