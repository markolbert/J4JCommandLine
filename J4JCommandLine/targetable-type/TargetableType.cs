using System;
using System.Reflection;

namespace J4JSoftware.CommandLine.Deprecated
{
    // abstract base class of the various ITargetableType classes
    public abstract class TargetableType : ITargetableType
    {
        protected TargetableType( 
            Type type, 
            PropertyMultiplicity multiplicity
            )
        {
            SupportedType = type;
            ParameterlessConstructor = type.GetConstructor( Type.EmptyTypes );
            Multiplicity = multiplicity;
        }

        // the SupportedType's public parameterless constructor or null if it doesn't have one
        protected ConstructorInfo? ParameterlessConstructor { get; }

        // the Type supported/described by this instance
        public Type SupportedType { get; }

        public bool IsCollection =>
            Multiplicity == PropertyMultiplicity.Array || Multiplicity == PropertyMultiplicity.List;

        public bool HasPublicParameterlessConstructor => ParameterlessConstructor != null;
        public PropertyMultiplicity Multiplicity { get; }

        // the ITextConverter used to create an instance of the SupportedType (or collection of
        // SupportedType) from text value(s)
        public ITextConverter? Converter { get; protected set; }
        public bool IsCreatable { get; protected set; }

        public abstract object? GetDefaultValue();
    }
}