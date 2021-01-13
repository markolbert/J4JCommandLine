using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace J4JSoftware.Configuration.CommandLine
{
    public class DefaultConverter : IConverter
    {
        private Dictionary<Type, MethodInfo> _supportedTypes = new();

        public DefaultConverter()
        {
            foreach( var methodInfo in typeof(Convert).GetMethods( BindingFlags.Static | BindingFlags.Public )
                .Where( m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 && !typeof(string).IsAssignableFrom( parameters[ 0 ].ParameterType );
                } ) )
            {
                if( _supportedTypes.ContainsKey( methodInfo.ReturnType ) )
                    _supportedTypes[ methodInfo.ReturnType ] = methodInfo;
                else _supportedTypes.Add( methodInfo.ReturnType, methodInfo );
            }
        }

        public bool CanConvert( Type toCheck )
        {
            // we support simple types, Lists of simple types and Arrays of simple types
            if( toCheck.IsArray )
            {
                var elementType = toCheck.GetElementType()!;
                return _supportedTypes.ContainsKey( elementType ) || elementType.IsEnum;
            }

            if( toCheck.IsGenericType )
            {

                if( toCheck.GenericTypeArguments.Length != 1 )
                    return false;

                var genType = toCheck.GetGenericArguments()[ 0 ];

                if( !typeof(List<>).MakeGenericType( genType ).IsAssignableFrom( toCheck ) )
                    return false;

                return !genType.IsGenericType && !genType.IsArray;
            }

            // now check for supported simple types
            return _supportedTypes.ContainsKey( toCheck ) || toCheck.IsEnum;
        }

        public IEnumerable<T?> Convert<T>( IEnumerable<string> values )
        {
            foreach( var converted in Convert( typeof(T), values ) )
            {
                yield return converted == null ? default(T) : (T) converted;
            }
        }

        public IEnumerable<object?> Convert( Type targetType, IEnumerable<string> values )
        {
            if (!CanConvert(targetType))
                yield break;

            if( targetType.IsEnum )
            {
                foreach( var text in values )
                {
                    yield return Enum.TryParse( targetType, text, true, out var convEnum ) ? convEnum : default;
                }
            }
            else
            {
                foreach( var text in values )
                {
                    yield return _supportedTypes[ targetType ].Invoke( null, new object[] { text } );
                }
            }
        }
    }
}