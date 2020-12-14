using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace J4JSoftware.CommandLine
{
    public interface IElementTerminator
    {
        int GetMaxTerminatorLength( string text, bool isKey );
    }
}