using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IAllocations : ICollection<IAllocation>
    {
        IAllocation Unkeyed { get; }
        IAllocation this [ string key ] { get; }
        IAllocation this [ int idx ] { get; }

        bool Contains( string key );
    }
}