using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IElementKey
    {
        int GetMaxPrefixLength( ElementProcessor processor );
    }
}
