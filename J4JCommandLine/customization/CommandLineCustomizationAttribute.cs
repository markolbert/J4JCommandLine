using System;

namespace J4JSoftware.Configuration.CommandLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandLineCustomizationAttribute : Attribute
    {
        public CommandLineCustomizationAttribute(
            Customization customization,
            int priority
        )
        {
            Customization = customization;
            Priority = priority;
        }

        public Customization Customization { get; }
        public int Priority { get; }
    }
}