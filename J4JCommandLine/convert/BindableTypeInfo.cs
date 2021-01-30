using System;
using System.Collections.Generic;
#pragma warning disable 8618

namespace J4JSoftware.Configuration.CommandLine
{
    public class BindableTypeInfo
    {
        public static BindableTypeInfo Create( Type toCheck )
        {
            if( toCheck.IsArray )
                return new BindableTypeInfo
                {
                    BindableType = BindableType.Array, 
                    TargetType = toCheck.GetElementType()!
                };

            // if it's not an array and not a generic it's a "simple" type
            if( !toCheck.IsGenericType )
                return new BindableTypeInfo
                {
                    BindableType = BindableType.Simple,
                    TargetType = toCheck
                };

            if (toCheck.GenericTypeArguments.Length != 1)
                return new BindableTypeInfo
                {
                    BindableType = BindableType.Unsupported,
                    TargetType = toCheck
                };

            var genType = toCheck.GetGenericArguments()[0];

            return typeof(List<>).MakeGenericType( genType ).IsAssignableFrom( toCheck )
                ? new BindableTypeInfo
                {
                    BindableType = BindableType.List,
                    TargetType = genType
                }
                : new BindableTypeInfo
                {
                    BindableType = BindableType.Unsupported,
                    TargetType = toCheck
                };
        }

        private BindableTypeInfo()
        {
        }

        public BindableType BindableType { get; private set; }
        public Type TargetType { get; private set; }
    }
}