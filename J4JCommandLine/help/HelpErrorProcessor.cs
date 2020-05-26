namespace J4JSoftware.CommandLine
{
    // A basic implementation of IHelpErrorProcessor. Doesn't do anything by itself, instead
    // serving as a starting point for customized handlers (e.g., to output errors and help information
    // to the console).
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

        // the result obtained from parsing the command line arguments
        protected MappingResults Result { get; private set; }

        // the IBindingTarget that called the HelpErrorProcessor
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
