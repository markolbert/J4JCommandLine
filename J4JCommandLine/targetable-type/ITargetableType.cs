using System;

namespace J4JSoftware.CommandLine
{
    // information about a Type used to determine if it is targetable by the framework,
    // converting values from text to the SupportedType, and getting a default value for the
    // SupportedType
    public interface ITargetableType
    {
        // the Type described by the ITargetableType instance
        Type SupportedType { get; }

        bool HasPublicParameterlessConstructor { get; }
        PropertyMultiplicity Multiplicity { get; }

        // the ITextConverter used to create an instance of the SupportedType (or collection of
        // SupportedType) from text value(s)
        ITextConverter? Converter { get; }
        bool IsCreatable { get; }
        bool IsCollection { get; }

        object? GetDefaultValue();
    }
}
