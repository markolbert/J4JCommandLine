using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.CommandLine
{
    public class CommandLineLogger : IEnumerable<string>
    {
        private readonly List<string> _messages = new List<string>();

        public bool HasMessages => _messages.Count > 0;

        public void Log( string mesg ) => _messages.Add( mesg );
        public IEnumerator<string> GetEnumerator()
        {
            foreach( var mesg in _messages )
            {
                yield return mesg;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
