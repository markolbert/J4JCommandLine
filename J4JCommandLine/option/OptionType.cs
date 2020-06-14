namespace J4JSoftware.CommandLine
{
    // Mappable options can be mapped to target properties. Null options can collect
    // error information but cannot be mapped to target properties.
    public enum OptionType
    {
        Keyed,
        Unkeyed,
        Null
    }
}