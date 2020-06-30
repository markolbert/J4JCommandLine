using System;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public class Allocations : KeyedCollection<string, IAllocation>, IAllocations
    {
        public Allocations( StringComparison keyComp )
            : base( keyComp.ToStringComparer() )
        {
            Unkeyed = new Allocation( this );
        }

        // the parameters from the command line not associated with any
        // keyed option
        public IAllocation Unkeyed { get; }

        protected override string GetKeyForItem( IAllocation item )
        {
            return item.Key!;
        }
    }
}