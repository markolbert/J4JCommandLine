using System;

namespace J4JSoftware.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class OptionRequiredAttribute : Attribute
    {
        public OptionRequiredAttribute(bool isRequired)
        {
            IsRequired = isRequired;
        }

        public bool IsRequired { get; }
    }
}