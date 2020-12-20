using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public class CommandLineLogger
    {
        private readonly List<string> _messages = new List<string>();

        public void Log( string mesg ) => _messages.Add( mesg );
    }
}
