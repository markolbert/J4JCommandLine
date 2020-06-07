using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class OptionCollection : IEnumerable<Option> 
    {
        private readonly List<Option> _options = new List<Option>();
        private readonly StringComparison _keyComp;
        private readonly UniqueText _helpKeys;

        public OptionCollection( StringComparison keyComp, UniqueText helpKeys )
        {
            _keyComp = keyComp;
            _helpKeys = helpKeys;
        }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool HasKey( string key )
        {
            if( _options.Any( opt => opt.Keys.Any( k => string.Equals( k, key, _keyComp ) ) ) )
                return true;

            if( _helpKeys.HasText( key ) )
                return true;

            return false;
        }

        // eliminates duplicate keys from a collection based on the case sensitivity rules
        public string[] GetUniqueKeys( params string[] keys ) =>
            keys.Where( k => !HasKey( k ) )
                .ToArray();

        public Option? this[ string key ] =>
            _options.FirstOrDefault( opt =>
                opt.Keys.Any( k => string.Equals( k, key, _keyComp ) ) );

        public bool Add( Option option )
        {
            foreach( var key in option.Keys )
            {
                if( HasKey(key) )
                    return false;
            }

            _options.Add( option );

            return true;
        }

        public void Clear() => _options.Clear();

        public IEnumerator<Option> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}