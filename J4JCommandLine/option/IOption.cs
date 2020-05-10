using System;
using System.Collections.ObjectModel;
using System.Reflection.Metadata;

namespace J4JSoftware.CommandLine
{
    public interface IOption
    {
        ReadOnlyCollection<string> Keys { get; }
        bool IsValid( object toCheck );
    }

    public interface IOption<TOption> : IOption
    {
        Func<TOption>? GetDefaultValue { get; }
        Func<TOption, bool>? Validator { get; }

        IOption<TOption> AddKey( string key );
        IOption<TOption> SetDefaultValue( TOption defaultValue );
        IOption<TOption> SetValidator( Func<TOption, bool> validator );
        bool IsValid( TOption toCheck );
    }
}