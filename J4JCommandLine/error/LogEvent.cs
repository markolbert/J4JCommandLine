using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace J4JSoftware.CommandLine.Deprecated
{
    // class for capturing logger, keyed by the command line key associated with an
    // error (i.e., the 'x' in '-x').
    //
    // Keys in the error collection follow the case-sensitivity rule set for the overall 
    // parsing effort (defined in IParsingConfiguration)
    public class LogEvent
    {
        public LogEvent( ProcessingPhase phase, string text )
        {
            Phase = phase;
            Text = text;
        }

        public ProcessingPhase Phase { get; }
        public string Text { get; }

        public Option? Option { get; set; }
        public TargetedProperty? Property { get; set; }

        public string FirstKey => Option?.FirstKey ?? string.Empty;

        public List<string> GetKeyDisplay( MasterTextCollection masterText )
        {
            var retVal = new List<string>();

            if( Option == null )
                return retVal;

            return Option.Keys.Aggregate(
                retVal,
                ( list, key ) =>
                {
                    list.AddRange(
                        masterText[ TextUsageType.Prefix ].Aggregate(
                            new List<string>(),
                            ( innerList, delim ) =>
                            {
                                innerList.Add( $"{delim}{key}" );

                                return innerList;
                            }
                        )
                    );

                    return list;
                } );
        }

        public string ToString( bool inclProperty )
        {
            var sb = new StringBuilder( $"[{Phase}] {Text}" );

            if( inclProperty && Property != null )
            {
                if( sb.Length > 0 )
                    sb.Append( " " );

                sb.Append( $"[{Property.FullPath}]" );
            }

            return sb.ToString();
        }

        public override string ToString() => ToString( false );
    }
}