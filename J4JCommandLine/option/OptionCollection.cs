using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class OptionCollection : IOptionCollection
    {
        private readonly List<IOption> _options = new List<IOption>();
        private readonly IParsingConfiguration _parseConfig;

        public OptionCollection( IParsingConfiguration parseConfig )
        {
            _parseConfig = parseConfig;
        }

        public ReadOnlyCollection<IOption> Options => _options.AsReadOnly();

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool HasKey( string key )
        {
            if( _options.Any( opt => opt.Keys.Any( k => string.Equals( k, key, _parseConfig.TextComparison ) ) ) )
                return true;

            return false;
        }

        // eliminates duplicate keys from a collection based on the case sensitivity rules
        public string[] GetUniqueKeys( params string[] keys ) =>
            keys.Where( k => !HasKey( k ) )
                .ToArray();

        public IOption? this[ string key ] =>
            _options.FirstOrDefault( opt =>
                opt.Keys.Any( k => string.Equals( k, key, _parseConfig.TextComparison ) ) );

        public bool Add( IOption option )
        {
            foreach( var key in option.Keys )
                if( _options.Any( opt => opt.Keys.Any( k => string.Equals( k, key, _parseConfig.TextComparison ) ) ) )
                    return false;

            _options.Add( option );

            return true;
        }

        public IEnumerator<IOption> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}