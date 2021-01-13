using System;
using System.Collections.Generic;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IConverters
    {
        bool CanConvert(Type toCheck);
        IEnumerable<object?> Convert( Type targetType, IEnumerable<string> values );
        IEnumerable<T?> Convert<T>(IEnumerable<string> values);
    }
}