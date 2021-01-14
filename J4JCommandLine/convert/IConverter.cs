using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IConverter
    {
        bool CanConvert( Type toCheck );
        object? Convert(Type targetType, IEnumerable<string> values);
        T? Convert<T>( IEnumerable<string> values );
    }
}
