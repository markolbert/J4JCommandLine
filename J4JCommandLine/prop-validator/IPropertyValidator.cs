using System.Collections.Generic;
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface IPropertyValidator
    {
        bool IsPropertyBindable( Stack<PropertyInfo> propertyStack );
    }
}