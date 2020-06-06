using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // A basic implementation of IHelpErrorProcessor. Doesn't do anything by itself, instead
    // serving as a starting point for customized handlers (e.g., to output errors and help information
    // to the console).
    public abstract class HelpErrorProcessor : IHelpErrorProcessor
    {
        private CommandLineErrors _errors;

        protected HelpErrorProcessor()
        {
        }

        public UniqueText HelpKeys { get; private set; }
        public bool IsInitialized => HelpKeys?.Count() > 0;

        public bool Initialize( 
            StringComparison keyComp, 
            CommandLineErrors errors,
            IElementKey prefixer, 
            params string[] helpKeys )
        {
            _errors = errors;

            if( !prefixer.IsInitialized )
                _errors.AddError( null, 
                    null,
                    $"Attempting to use uninitialized {nameof(IElementKey.Prefixes)} to initialize {nameof(HelpErrorProcessor)}" );

            Prefixes = prefixer.Prefixes;

            HelpKeys = new UniqueText(keyComp);
            HelpKeys.AddRange( helpKeys );

            if( !IsInitialized )
                _errors.AddError(null,
                    null,
                    "No help keys were specified");

            return prefixer.IsInitialized && IsInitialized;
        }

        // the result obtained from parsing the command line arguments
        protected MappingResults Result { get; private set; }

        protected UniqueText Prefixes { get; private set; }

        // the IBindingTarget that called the HelpErrorProcessor
        protected IBindingTarget BindingTarget { get; private set; }
        
        protected bool HasErrors => ( Result & ~MappingResults.HelpRequested ) != MappingResults.Success;
        protected bool HelpRequested => (Result & MappingResults.HelpRequested) == MappingResults.HelpRequested;
        protected bool HasHeader => !string.IsNullOrEmpty(BindingTarget?.ProgramName)
                                  || !string.IsNullOrEmpty(BindingTarget?.Description);

        // displays errors and/or help depending upon what the binding and parsing process
        // produced and the user requested
        public virtual void Display( MappingResults result, IBindingTarget bindingTarget )
        {
            if( !IsInitialized )
            {
                _errors.AddError( bindingTarget, null, $"{nameof(HelpErrorProcessor)} is not initialized" );
                return;
            }

            Result = result;
            BindingTarget = bindingTarget;

            if (!HasErrors && !HelpRequested)
                return;

            InitializeOutput();

            if( HasHeader )
                CreateHeaderSection();

            if (HasErrors)
                CreateErrorSection();

            if (HelpRequested || HasErrors)
                CreateHelpSection();

            DisplayOutput();
        }

        protected virtual void InitializeOutput()
        {
        }

        protected abstract void CreateHeaderSection();
        protected abstract void CreateErrorSection();
        protected abstract void CreateHelpSection();
        protected abstract void DisplayOutput();
    }
}
