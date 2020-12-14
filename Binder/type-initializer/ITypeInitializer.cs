using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public interface ITypeInitializer
    {
        List<PropertyKey> GetContextKeys<TTarget>()
            where TTarget : class;
    }
}
