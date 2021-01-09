using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IValidationEntry
    {
        string EntryPath { get; }
        ReadOnlyCollection<ValidationError> Errors { get; }
        bool IsValid { get; }

        ValidationEntry<TEntry> CreateChild<TEntry>( TEntry toValidate, string? hint = null );
    }
}