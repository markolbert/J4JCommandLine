using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        string ID { get; }

        void AddError( string key, string error );
        MappingResults MapParseResults( ParseResults parseResults );
        object GetValue();
    }

    public interface IBindingTarget<TTarget> : IBindingTarget
        where TTarget : class
    {
        TTarget Value { get; }

        IOption<TProp>? BindProperty<TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            TProp defaultValue,
            params string[] keys );
    }
}