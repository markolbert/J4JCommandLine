using System;
using System.Collections.Generic;

namespace J4JCommandLine.Tests
{
    public class TextConverter
    {
        private readonly Dictionary<Type, Func<string, object?>> _converters =
            new Dictionary<Type, Func<string, object?>>();

        public TextConverter()
        {
            Case<int>( System.Convert.ToInt32 );
            Case<double>( System.Convert.ToDouble );
            Case<bool>( System.Convert.ToBoolean );
            Case<string>( x => x );
            Case<decimal>( System.Convert.ToDecimal );
        }

        public TextConverter Case<T>( Func<string, T> converter )
        {
            var type = typeof(T);

            if( _converters.ContainsKey( type ) )
                _converters[ type ] = x => converter( x );
            else _converters.Add( typeof(T), x => converter( x ) );

            return this;
        }

        public T Convert<T>( string x )
        {
            return (T) Convert( typeof(T), x );
        }

        public object Convert( Type targetType, string x )
        {
            if( !_converters.ContainsKey( targetType ) )
                return default!;

            return ( _converters[ targetType ]( x )! );
        }
    }
}