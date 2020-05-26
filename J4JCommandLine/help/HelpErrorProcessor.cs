using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public class HelpErrorProcessor : IHelpErrorProcessor
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

        protected MappingResults Result { get; private set; }
        protected IBindingTarget BindingTarget { get; private set; }
        
        protected bool HasErrors => ( Result & ~MappingResults.HelpRequested ) != MappingResults.Success;
        protected bool HelpRequested => (Result & MappingResults.HelpRequested) == MappingResults.HelpRequested;

        public virtual void Display( MappingResults result, IBindingTarget bindingTarget )
        {
            Result = result;
            BindingTarget = bindingTarget;
        }
    }
}
