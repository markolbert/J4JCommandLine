using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IConverter
    {
        bool CanConvert( Type toCheck );
        IEnumerable<object?> Convert(Type targetType, IEnumerable<string> values);
        IEnumerable<T?> Convert<T>( IEnumerable<string> values );
    }
}
