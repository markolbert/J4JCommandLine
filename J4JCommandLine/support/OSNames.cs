using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    public class OSNames
    {
        public const string Linux = "Linux";
        public const string Windows = "Windows";
        public const string Universal = "Universal";
        public const string Undefined = "Undefined";

        public static string[] Supported = new[] { Linux, Windows };
    }
}
