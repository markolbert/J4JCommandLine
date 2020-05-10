using System;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface ITargetableType
    {
        Type Type { get; }
    }

    public interface ITargetableType<T> : ITargetableType
    {
        bool Convert( string textValue, out T result );
        bool Convert( List<string> textValues, out List<T> result );
    }
}