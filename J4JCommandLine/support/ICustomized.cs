using System;

namespace J4JSoftware.Configuration.CommandLine
{
    public interface ICustomized
    {
        string OperatingSystem { get; }
        Customization Customization { get; }
        int Priority { get; }
        StringComparison TextComparison { get; }
    }
}