using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandLineOperatingSystemAttribute : Attribute
    {
        public CommandLineOperatingSystemAttribute(
            string osName,
            StringComparison textComparison
        )
        {
            OperatingSystem = osName;
            TextComparison = textComparison;
        }

        public string OperatingSystem { get; }
        public StringComparison TextComparison { get; }
    }
}
