using System.ComponentModel;

namespace J4JSoftware.Configuration.CommandLine
{
    public enum OptionStyle
    {
        Undefined,
        Switch,
        SingleValued,

        // for non-collection properties which expect multiple values
        // to be parsed (e.g., flag enums)
        ConcatenatedSingleValue,

        Collection
    }
}