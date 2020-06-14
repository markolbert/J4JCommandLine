using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IParseResults : ICollection<IParseResult>
    {
        IParseResult Unkeyed { get; }
    }

    public class ParseResults : KeyedCollection<string, IParseResult>, IParseResults
    {
        public ParseResults( StringComparison keyComp )
            : base( keyComp.ToStringComparer() )
        {
            Unkeyed = new ParseResult( this );
        }

        // the parameters from the command line not associated with any
        // keyed option
        public IParseResult Unkeyed { get; }

        protected override string GetKeyForItem( IParseResult item )
        {
            return item.Key;
        }
    }
}