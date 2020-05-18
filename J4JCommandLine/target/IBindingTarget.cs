using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        string ID { get; }
        ReadOnlyCollection<TargetableProperty> TargetableProperties { get; }

        OptionBase BindProperty(
            string propertyPath,
            object defaultValue,
            params string[] keys);

        OptionBase BindPropertyCollection(
            string propertyPath,
            params string[] keys );

        void AddError( string key, string error );
        MappingResults MapParseResults( ParseResults parseResults );
        object GetValue();
    }

    public interface IBindingTarget<TValue> : IBindingTarget
        where TValue : class
    {
        TValue Value { get; }

        OptionBase BindProperty<TProp>(
            Expression<Func<TValue, TProp>> propertySelector,
            object? defaultValue,
            params string[] keys );
    }
}