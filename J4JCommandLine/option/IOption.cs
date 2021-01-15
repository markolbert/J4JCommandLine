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
        IOption AddCommandLineKey(string cmdLineKey);
        IOption AddCommandLineKeys(IEnumerable<string> cmdLineKeys);
        string? CommandLineKeyProvided { get; set; }

        OptionStyle Style { get; }
        IOption SetStyle(OptionStyle style);

        ReadOnlyCollection<string> Values { get; }
        void AddValue(string value);
        void AddValues(IEnumerable<string> values);
        void ClearValues();
        int MaxValues { get; }
        int NumValuesAllocated { get; }
        bool ValuesSatisfied { get; }
        
        bool Required { get; }
        IOption IsRequired();
        IOption IsOptional();

        string? Description { get; }
        IOption SetDescription( string description );

        string? GetDefaultValue();
    }

    public interface IOption<T> : IOption
    {
        T? DefaultValue { get; }
        IOption<T> SetDefaultValue( T? value );
    }

    public interface ITypeBoundOption : IOption
    {
        Type ContainerType { get; }
    }
}