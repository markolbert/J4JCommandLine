using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IOption
    {
        bool IsInitialized { get; }
        OptionCollection Container { get; }
        string? ContextPath { get; }
        ReadOnlyCollection<string> Keys { get; }
        string? CommandLineKeyProvided { get; set; }
        int MaxValues { get; }
        int NumValuesAllocated { get; }
        bool ValuesSatisfied { get; }
        ReadOnlyCollection<string> CommandLineValues { get; }
        void AddAllocatedValue( string value );
        OptionStyle Style { get; }
        bool Required { get; }
        string? Description { get; }
        Option AddCommandLineKey(string cmdLineKey);
        Option AddCommandLineKeys(IEnumerable<string> cmdLineKeys);
        Option SetStyle( OptionStyle style );
        Option IsRequired();
        Option IsOptional();
        Option SetDescription(string description);
    }

    public interface ITypeBoundOption : IOption
    {
        Type TargetType { get; }
    }

}