using System;

namespace J4JSoftware.CommandLine
{
    public interface ITargetingConfiguration
    {
        IServiceProvider? ServiceProvider { get; }
        TargetableTypes TargetableTypes { get; }
        bool CanTarget( Type toTarget );
        bool CanCreate( Type toCreate );
    }
}