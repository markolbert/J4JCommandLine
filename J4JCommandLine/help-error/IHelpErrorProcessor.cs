using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IHelpErrorProcessor
    {
        UniqueText HelpKeys { get; }
        bool IsInitialized { get; }

        bool Initialize( StringComparison keyComp, CommandLineErrors errors, IElementKey prefixer, params string[] helpKeys );
        void Display( MappingResults results, IBindingTarget bindingTarget );
    }
}