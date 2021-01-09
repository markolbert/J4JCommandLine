using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Configuration.CommandLine
{
    internal static class InternalExtensions
    {
        public static void RemoveRange<T>( this List<T> list, IEnumerable<int> toRemove )
        {
            foreach( var idx in toRemove.Where( x => x < list.Count && x >= 0 )
                .OrderByDescending( x => x ) )
                list.RemoveAt( idx );
        }

        public static void RemoveFrom<T>( this List<T> list, int startIdx )
        {
            if( startIdx < 0 || startIdx >= list.Count )
                return;

            list.RemoveRange( Enumerable.Range( startIdx, list.Count - 1 ) );
        }
    }
}