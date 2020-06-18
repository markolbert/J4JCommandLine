using System;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    [ AttributeUsage( AttributeTargets.Property, AllowMultiple = false, Inherited = false ) ]
    public class OptionKeysAttribute : Attribute
    {
        public OptionKeysAttribute( params string[] keys )
        {
            Keys = keys;
        }

        public string[] Keys { get; }
    }
}