﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IOptionCollection : IEnumerable<IOption>
    {
        ReadOnlyCollection<IOption> Options { get; }
        IOption? this[ string key ] { get; }
        bool HasKey( string key );
        void Clear();
        bool Add( IOption option );
    }
}