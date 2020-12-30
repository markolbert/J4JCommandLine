using System.Collections.Generic;
using System.Linq;
#pragma warning disable 8618

namespace J4JSoftware.Configuration.CommandLine
{
    public class CommandLineLogger
    {
        private readonly List<LogEntry> _messages = new();

        public bool HasMessages( bool errorsOnly = true )
        {
            return _messages.Any( m => m.IsError == errorsOnly );
        }

        public void LogError( string mesg )
        {
            _messages.Add( new LogEntry { IsError = true, Message = mesg } );
        }

        public void LogInformation( string mesg )
        {
            _messages.Add( new LogEntry { Message = mesg } );
        }

        public IEnumerable<string> GetMessages( bool errorsOnly = true )
        {
            foreach( var entry in _messages.Where( m => m.IsError == errorsOnly ) )
                yield return $"[{( entry.IsError ? "Error" : "Information" )}]:{entry.Message}";
        }
    }
}