namespace J4JSoftware.CommandLine
{
    // the kinds of property multiplicity for properties that can be the targets of command line options
    public enum PropertyMultiplicity
    {
        // a ValueType, string or class with a public parameterless constructor that
        // is not one of the supported collection types
        SimpleValue,

        // an Array of SimpleValues
        Array,

        // a List<> of SimpleValues
        List,

        // indicates that the targeted property is not supported by the framework
        Unsupported
    }
}