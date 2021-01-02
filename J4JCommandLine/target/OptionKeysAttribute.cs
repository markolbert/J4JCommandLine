using System;

namespace J4JSoftware.CommandLine.Deprecated
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