using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // a collection of immutable text collections. Used to ensure no prefix, key,
    // value encloser, etc., has more than one role in the framework
    public partial class ImmutableTextCollections
    {
        private static StringComparison _textComp;
        private static List<ImmutableTextCollection> _collections = new List<ImmutableTextCollection>();

        public ImmutableTextCollections( StringComparison textComp )
        {
            _textComp = textComp;
        }

        public static ImmutableTextCollection Create( IEnumerable<string> items )
        {
            var retVal = new ImmutableTextCollection( _textComp, _collections, items );
        }

        public bool Contains( string item )
        {
            foreach( var collection in _collections )
            {
                if( collection.Contains( item ) )
                    return true;
            }

            return false;
        }
    }
}