using System;
using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine;

public class UndefinedTextToValue : ITextToValue
{
    public Type TargetType => typeof( object );
    public bool CanConvert( Type toCheck ) => false;

    public bool Convert( Type targetType, IEnumerable<string> values, out object? result )
    {
        result = null;
        return false;
    }

    public bool Convert<T>( IEnumerable<string> values, out T? result )
    {
        result = default;
        return false;
    }
}
