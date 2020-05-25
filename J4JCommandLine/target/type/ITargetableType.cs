using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace J4JSoftware.CommandLine
{
    public interface ITargetableType
    {
        Type SupportedType { get; }
        ConstructorInfo? ParameterlessConstructor { get; }
        bool HasPublicParameterlessConstructor { get; }
        PropertyMultiplicity Multiplicity { get; }
        ITextConverter? Converter { get; }
        bool IsCreatable { get; }

        object? GetDefaultValue();
    }
}
