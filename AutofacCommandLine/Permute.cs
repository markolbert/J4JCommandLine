using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace J4JSoftware.Configuration.CommandLine
{
    // thanx to https://gist.github.com/fdeitelhoff/5052484 for this!
    public static class PermuteExtensions
    {
        public static IEnumerable<IEnumerable<T>> Permute<T>( this IList<T> values )
        {
            ICollection<IList<T>> result = new List<IList<T>>();

            Permute( values, values.Count, result );

            return result;
        }

        private static void Permute<T>( IList<T> values, int n, ICollection<IList<T>> result )
        {
            if( n == 1 )
                result.Add( new List<T>( values ) );
            else
            {
                for( var i = 0; i < n; i++ )
                {
                    Permute( values, n - 1, result );
                    Swap( values, n % 2 == 1 ? 0 : i, n - 1 );
                }
            }
        }

        private static void Swap<T>( IList<T> values, int i, int j ) =>
            ( values[ i ], values[ j ] ) = ( values[ j ], values[ i ] );
    }
}
