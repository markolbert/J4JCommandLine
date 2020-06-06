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
        private readonly StringComparison _textComp;

        private List<string> _elements = new List<string>();

        public UniqueText( StringComparison textComp )
        {
            _textComp = textComp;
        }

        public ReadOnlyCollection<string> Elements => _elements.AsReadOnly();

        public bool HasText( string element ) =>
            _elements.Any( e => string.Equals( e, element, _textComp ) );

        public void Add( string toAdd )
        {
            if( !HasText( toAdd ) )
                _elements.Add( toAdd );
        }

        public void AddRange(params string[] range)
        {
            _elements.AddRange(range);

            _elements = _elements.Distinct( _textComp.ToStringComparer() )
                .ToList();
        }

        public void AddRange( IEnumerable<string> range )
        {
            _elements.AddRange(range);

            _elements = _elements.Distinct(_textComp.ToStringComparer())
                .ToList();
        }

        public void Remove(string toRemove)
        {
            if (!HasText(toRemove))
                _elements.Remove(toRemove);
        }

        public void Remove( IEnumerable<string> range )
        {
            foreach( var toRemove in range )
            {
                var idx = _elements.FindIndex( x => string.Equals( x, toRemove, _textComp ) );

                if( idx >= 0 )
                    _elements.RemoveAt( idx );
            }
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