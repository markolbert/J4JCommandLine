using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace J4JSoftware.CommandLine
{
    public class CommandLineLogger
    {
        public class ConsolidatedLog
        {
            public List<string> Keys { get; set; }
            public List<string> Texts { get; set; }
        }

        private readonly StringComparison _keyComp;
        private readonly List<LogEvent> _events = new List<LogEvent>();

        public CommandLineLogger( StringComparison keyComp )
        {
            _keyComp = keyComp;
        }

        public void Clear( params ProcessingPhase[] phases )
        {
            if( phases == null || phases.Length == 0 )
                _events.Clear();
            else
                _events.RemoveAll( e => phases.Any( p => p == e.Phase ) );
        }

        public int Count => _events.Count;

        public void LogError( 
            ProcessingPhase phase,
            string text, 
            Option? option = null, 
            TargetedProperty? property = null )
        {
            _events.Add(new LogEvent( phase, text )
            {
                Option = option,
                Property = property
            } );
        }

        public IEnumerable<ConsolidatedLog> ConsolidateLogEvents( MasterTextCollection masterText, bool inclProperty = true )
        {
            foreach( var group in _events.GroupBy( e => e.FirstKey ) )
            {
                yield return new ConsolidatedLog
                {
                    Keys = group.First().GetKeyDisplay( masterText ),
                    Texts = group.OrderBy(g=>g.Phase)
                        .Select( x => x.ToString( inclProperty ) )
                        .ToList()
                };
            }
        }
    }
}