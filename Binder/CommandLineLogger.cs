using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class CommandLineLogger
    {
        public class LogEntry
        {
            public bool IsError { get; set; }
            public string Message { get; set; }
        }
        
        private readonly List<LogEntry> _messages = new List<LogEntry>();

        public bool HasMessages( bool errorsOnly = true ) => _messages.Any( m => m.IsError == errorsOnly );

        public void LogError( string mesg ) => _messages.Add( new LogEntry { IsError = true, Message = mesg } );
        public void LogInformation( string mesg) => _messages.Add(new LogEntry { Message = mesg });

        public IEnumerable<string> GetMessages( bool errorsOnly = true )
        {
            foreach( var entry in _messages.Where(m=>m.IsError == errorsOnly  ) )
            {
                yield return $"[{(entry.IsError ? "Error" : "Information")}]:{entry.Message}" ;
            }
        }
    }
}
