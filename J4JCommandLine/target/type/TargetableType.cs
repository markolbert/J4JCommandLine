using System;
using System.Collections.Generic;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
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

        public Type SupportedType { get; }
        public ConstructorInfo? ParameterlessConstructor { get; }
        public bool HasPublicParameterlessConstructor => ParameterlessConstructor != null;
        public PropertyMultiplicity Multiplicity { get; }
        public ITextConverter? Converter { get; protected set; }
        public bool IsCreatable { get; protected set; }
        public abstract object? GetDefaultValue();
    }
}