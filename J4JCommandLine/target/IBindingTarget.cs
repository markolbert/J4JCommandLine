using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        ReadOnlyCollection<TargetedProperty> TargetedProperties { get; }
        IOptionCollection Options { get; }
        CommandLineErrors Errors { get; }

        void AddError( string key, string error );
        MappingResults Parse(string[] args);
        object GetValue();
    }

    public interface IBindingTarget<TValue> : IBindingTarget
        where TValue : class
    {
        TValue Value { get; }

        OptionBase Bind<TProp>(
            Expression<Func<TValue, TProp>> propertySelector,
            params string[] keys );
    }
}