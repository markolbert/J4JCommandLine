using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.CommandLine
{
    public class OptionConfiguration
    {
        public OptionConfiguration( 
            PropertyInfo propInfo, 
            Stack<PropertyInfo> propStack, 
            string[]? keys )
        {
            PropertyInfoPath = propStack.ToList();
            PropertyInfoPath.Reverse();
            PropertyInfoPath.Add( propInfo );

            if( keys?.Length > 0 )
                Keys = keys;

            var required = propInfo.GetCustomAttribute<OptionRequiredAttribute>();
            if (required != null)
                IsRequired = required.IsRequired;

            var defaultValue = propInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultValue != null)
                DefaultValue = defaultValue.Value;

            var description = propInfo.GetCustomAttribute<DescriptionAttribute>();
            if( description != null )
                Description = description.Description;
        }

        public List<PropertyInfo> PropertyInfoPath { get; }
        public string[]? Keys { get; }
        public bool Unkeyed => Keys?.Length == null;
        public bool IsRequired { get; }
        public object? DefaultValue { get; }
        public string? Description { get; }
    }
}