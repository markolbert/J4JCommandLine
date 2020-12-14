using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public interface IKeyPrefixer
    {
        int GetMaxPrefixLength( string text );
    }
}
