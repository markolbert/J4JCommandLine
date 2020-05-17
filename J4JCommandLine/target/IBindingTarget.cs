using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        string ID { get; }

        void AddError( string key, string error );
        bool MapParseResults( ParseResults parseResults );
    }

    public interface IBindingTarget<TTarget> : IBindingTarget
        where TTarget : class
    {
        TTarget Target { get; }

        IOption<TProp>? BindProperty<TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            TProp defaultValue,
            params string[] keys );
    }
}