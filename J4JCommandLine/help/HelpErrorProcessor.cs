using System;

namespace J4JSoftware.CommandLine
{
    // A basic implementation of IHelpErrorProcessor. Doesn't do anything by itself, instead
    // serving as a starting point for customized handlers (e.g., to output errors and help information
    // to the console).
    public abstract class HelpErrorProcessor : IHelpErrorProcessor
    {
        protected HelpErrorProcessor( IParsingConfiguration parseConfig )
        {
            ParsingConfiguration = parseConfig;
        }

        protected IParsingConfiguration ParsingConfiguration { get; }

        // the result obtained from parsing the command line arguments
        protected MappingResults Result { get; private set; }

        // the IBindingTarget that called the HelpErrorProcessor
        protected IBindingTarget BindingTarget { get; private set; }
        
        protected bool HasErrors => ( Result & ~MappingResults.HelpRequested ) != MappingResults.Success;
        protected bool HelpRequested => (Result & MappingResults.HelpRequested) == MappingResults.HelpRequested;
        protected bool HasHeader => !string.IsNullOrEmpty(ParsingConfiguration.ProgramName)
                                  || !string.IsNullOrEmpty(ParsingConfiguration.Description);

        // displays errors and/or help depending upon what the binding and parsing process
        // produced and the user requested
        public virtual void Display( MappingResults result, IBindingTarget bindingTarget )
        {
            Result = result;
            BindingTarget = bindingTarget;

            if (!HasErrors && !HelpRequested)
                return;

            Initialize();

            if( HasHeader )
                CreateHeaderSection();

            if (HasErrors)
                CreateErrorSection();

            if (HelpRequested || HasErrors)
                CreateHelpSection();

            DisplayOutput();
        }

        protected virtual void Initialize()
        {
        }

        protected abstract void CreateHeaderSection();
        protected abstract void CreateErrorSection();
        protected abstract void CreateHelpSection();
        protected abstract void DisplayOutput();
    }
}
