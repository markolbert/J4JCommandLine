using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IOptionCollection : IEnumerable<IOption>
    {
        ReadOnlyCollection<IOption> Options { get; }
        IOption? this[ string key ] { get; }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        bool HasKey( string key );

        // eliminates duplicate keys from a collection based on the case sensitivity rules
        string[] GetUniqueKeys( params string[] keys );
        bool Add( IOption option );
    }
}