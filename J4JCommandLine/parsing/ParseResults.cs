using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public class ParseResults : KeyedCollection<string, IParseResult>
    {
        public ParseResults( IParsingConfiguration parsingConfig )
            : base( parsingConfig.TextComparison.ToStringComparer() )
        {
        }

        protected override string GetKeyForItem( IParseResult item )
        {
            return item.Key;
        }
    }
}