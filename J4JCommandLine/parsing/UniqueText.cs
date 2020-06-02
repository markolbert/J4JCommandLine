using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    // defines a class for holding unique text elements which can be used as 
    // prefixes, value enclosers, etc.
    public class UniqueText : IEnumerable<string>
    {
        private readonly List<string> _elements = new List<string>();
        private readonly StringComparison _textComp;

        public UniqueText( StringComparison textComp )
        {
            _textComp = textComp;
        }

        public ReadOnlyCollection<string> Elements => _elements.AsReadOnly();

        public bool HasElement( string element ) =>
            _elements.Any( e => string.Equals( e, element, _textComp ) );

        public bool Add( string toAdd )
        {
            if( HasElement( toAdd ) )
                return false;

            _elements.Add( toAdd );

            return true;
        }

        public bool Remove( string toRemove )
        {
            if( !HasElement( toRemove ) )
                return false;

            _elements.Remove( toRemove );

            return true;
        }

        public bool AddRange(IEnumerable<string> range )
        {
            var retVal = true;

            foreach( var toAdd in range )
            {
                if( HasElement( toAdd ) )
                    retVal = false;
                else _elements.Add( toAdd );
            }

            return retVal;
        }

        public bool Remove(IEnumerable<string> range)
        {
            var retVal = true;

            foreach( var toRemove in range )
            {
                if( !HasElement( toRemove ) )
                    retVal = false;
                else _elements.Remove(toRemove);
            }

            return retVal;
        }

        public void Clear()
        {
            _elements.Clear();
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach( var element in _elements )
            {
                yield return element;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}