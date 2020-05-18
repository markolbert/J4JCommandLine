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

        void AddError( string key, string error );
        MappingResults MapParseResults( ParseResults parseResults );
        object GetValue();
    }

    public interface IBindingTarget<TTarget> : IBindingTarget
        where TTarget : class
    {
        TTarget Value { get; }

        OptionBase BindProperty<TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            TProp defaultValue,
            params string[] keys );
    }
}