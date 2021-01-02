using System;

namespace J4JSoftware.CommandLine.Deprecated
{
    // describes the factory method used to create instances of ITargetableType from a Type
    public interface ITargetableTypeFactory
    {
        ITargetableType Create( Type type );
    }
}