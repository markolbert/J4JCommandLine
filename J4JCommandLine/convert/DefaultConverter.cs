using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            var collInfo = BindableTypeInfo.Create( toCheck );

            // we support simple types, Lists of simple types and Arrays of simple types
            return _supportedTypes.ContainsKey( collInfo.TargetType ) || collInfo.TargetType.IsEnum;
        }

        private object? ConvertSingleString( Type targetType, string? text )
        {
            // this check should never get triggered, but...
            if( !CanConvert( targetType ) )
                return null;

            if( string.IsNullOrEmpty( text ) )
                return null;

            if( targetType.IsEnum )
                return Enum.TryParse( targetType, text, true, out var convEnum ) ? convEnum : null;

            return _supportedTypes[ targetType ].Invoke( null, new object[] { text } );
        }

        public T? Convert<T>( IEnumerable<string> values )
        {
            var targetType = typeof(T);

            var retVal = Convert( targetType, values );

            if( retVal == null )
                return default(T);

            return (T) retVal;
        }

        public object? Convert( Type targetType, IEnumerable<string> values )
        {
            if( !CanConvert( targetType ) )
                return null;

            var listValues = values.ToList();

            var bindableInfo = BindableTypeInfo.Create( targetType );

            return bindableInfo.BindableType switch
            {
                BindableType.Array => ConvertToArray( targetType, listValues ),
                BindableType.List => ConvertToList( targetType, listValues ),
                BindableType.Simple => ConvertSingleString( targetType, listValues.FirstOrDefault() ),
                BindableType.Unsupported => null,
                _ => throw new InvalidEnumArgumentException(
                    $"Unsupported {typeof(BindableType)} '{bindableInfo.BindableType}'" )
            };
        }

        private object ConvertToArray(Type elementType, List<string> values)
        {
            var retVal = Array.CreateInstance(elementType, values.Count);

            for (var idx = 0; idx < values.Count; idx++)
            {
                retVal.SetValue(ConvertSingleString(elementType, values[idx]), idx);
            }

            return retVal;
        }

        private object ConvertToList(Type elementType, List<string> values)
        {
            var retVal = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

            foreach (var value in values)
            {
                retVal!.Add(ConvertSingleString(elementType, value));
            }

            return retVal;
        }

    }
}