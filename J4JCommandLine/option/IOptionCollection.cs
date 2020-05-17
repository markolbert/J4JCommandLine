using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IOptionCollection : IEnumerable<IOption>
    {
        ReadOnlyCollection<IOption> Options { get; }
        bool HasKey( string key );
        IOption? this[string key] { get; }
        void Clear();
        bool Add( IOption option );
    }
}