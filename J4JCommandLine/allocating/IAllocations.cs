using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IAllocations : ICollection<IAllocation>
    {
        IAllocation Unkeyed { get; }
    }
}