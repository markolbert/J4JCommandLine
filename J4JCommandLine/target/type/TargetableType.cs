using System;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public abstract class TargetableType : ITargetableType
    {
        protected TargetableType( Type type, PropertyMultiplicity multiplicity )
        {
            SupportedType = type;
            ParameterlessConstructor = type.GetConstructor( Type.EmptyTypes );
            Multiplicity = multiplicity;
        }

        public Type SupportedType { get; }
        public ConstructorInfo? ParameterlessConstructor { get; }
        public bool HasPublicParameterlessConstructor => ParameterlessConstructor != null;
        public PropertyMultiplicity Multiplicity { get; }
        public bool IsCreatable { get; protected set; }
        public abstract object? Create();
    }
}