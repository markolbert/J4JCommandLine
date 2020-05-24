using System;

namespace J4JSoftware.CommandLine
{
    public interface ITargetableTypeFactory
    {
        ITargetableType Create( Type type );
    }
}