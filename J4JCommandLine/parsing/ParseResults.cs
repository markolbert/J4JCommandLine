using System;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public class ParseResults : KeyedCollection<string, IParseResult>
    {
        public ParseResults( StringComparison keyComp )
            : base( keyComp.ToStringComparer() )
        {
        }

        protected override string GetKeyForItem( IParseResult item )
        {
            return item.Key;
        }
    }
}