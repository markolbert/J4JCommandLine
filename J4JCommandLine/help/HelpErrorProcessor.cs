using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public abstract class HelpErrorProcessor : IHelpErrorProcessor
    {
        protected HelpErrorProcessor(
            IParsingConfiguration parseConfig,
            IOutputConfiguration outputConfig
        )
        {
            ParsingConfiguration = parseConfig;
            OutputConfiguration = outputConfig;
        }

        protected IParsingConfiguration ParsingConfiguration { get; }
        protected IOutputConfiguration OutputConfiguration { get; }

        public abstract void Display( IOptionCollection options, CommandLineErrors errors, string? headerMesg = null );
    }
}
