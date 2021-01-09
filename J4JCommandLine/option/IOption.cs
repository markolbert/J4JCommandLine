using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IOption
    {
        bool IsInitialized { get; }
        IOptionCollection Container { get; }
        string? ContextPath { get; }
        ReadOnlyCollection<string> Keys { get; }
        string? CommandLineKeyProvided { get; set; }
        int MaxValues { get; }
        int NumValuesAllocated { get; }
        bool ValuesSatisfied { get; }
        ReadOnlyCollection<string> Values { get; }
        OptionStyle Style { get; }
        bool Required { get; }
        string? Description { get; }
        void ClearValues();
        void AddValue( string value );
        void AddValues( IEnumerable<string> values );
        Option AddCommandLineKey( string cmdLineKey );
        Option AddCommandLineKeys( IEnumerable<string> cmdLineKeys );
        Option SetStyle( OptionStyle style );
        Option IsRequired();
        Option IsOptional();
        Option SetDescription( string description );
    }

    public interface ITypeBoundOption : IOption
    {
        Type TargetType { get; }
    }
}