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
        private readonly MasterTextCollection _masterText;

        public OptionCollection( MasterTextCollection masterText )
        {
            _masterText = masterText;
        }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool HasKey( string key )
        {
            //if( _options.Any( opt => opt.Keys.Any( k => string.Equals( k, key, _keyComp ) ) ) )
            //    return true;
            if( _masterText.Contains( key, TextUsageType.OptionKey ) )
                return true;

            return false;
        }

        // eliminates duplicate keys from a collection based on the case sensitivity rules
        public string[] GetUniqueKeys( params string[] keys ) =>
            keys.Where( k => !HasKey( k ) )
                .ToArray();

        public Option? this[ string key ] =>
            _options.FirstOrDefault( opt =>
                opt.Keys.Any( k => string.Equals( k, key, _masterText.TextComparison ) ) );

        public bool Add( Option option )
        {
            foreach( var key in option.Keys )
            {
                if( _masterText.Contains(key) )
                    return false;
            }

            // only one unkeyed option is allowed because it corresponds to all of the
            // un-prefixed/unkeyed parameters
            if( option.OptionType == OptionType.Unkeyed )
            {
                var existing = _options.Where( opt => opt.OptionType == OptionType.Unkeyed )
                    .Select( ( opt, idx ) => (opt,idx) )
                    .FirstOrDefault();

                if( existing.opt != null )
                    _options.RemoveAt( existing.idx );
            }

            _options.Add( option );
            _masterText.AddRange( TextUsageType.OptionKey, option.Keys );

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