using System;

namespace J4JSoftware.CommandLine
{
    public interface ITypeBoundOption
    {
        Type TargetType { get; }
    }
}