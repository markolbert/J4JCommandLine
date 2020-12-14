using System;
using System.Collections.Generic;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public abstract class TypeInitializerBase : ITypeInitializer
    {
        protected TypeInitializerBase( IJ4JLogger logger )
        {
            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        public List<PropertyKey> GetContextKeys<TTarget>()
            where TTarget : class
        {
            var retVal = new List<PropertyKey>();

            AddProperties( typeof(TTarget).GetProperties(), null, retVal );

            return retVal;
        }

        private void AddProperties( IEnumerable<PropertyInfo> propInfoCollection, PropertyKey? parentKey, List<PropertyKey> contextKeys )
        {
            foreach( var propInfo in propInfoCollection )
            {
                if (!SupportedProperty(propInfo))
                    continue;

                var propKey = new PropertyKey( propInfo, parentKey );

                contextKeys.Add(propKey);

                AddProperties(propInfo.PropertyType.GetProperties(), propKey, contextKeys);
            }
        }

        protected abstract bool SupportedProperty( PropertyInfo propInfo );

        protected bool IsValueType( Type toCheck ) => typeof(ValueType).IsAssignableFrom( toCheck );

        protected bool HasParameterlessConstructor( Type toCheck ) =>
            toCheck.GetConstructor( new Type[] { } ) != null;

        protected bool IsArray( Type toCheck, out Type? elementType )
        {
            elementType = null;

            if( !toCheck.IsArray ) 
                return false;

            elementType = toCheck.GetElementType();
            
            return true;
        }

        protected bool IsList( Type toCheck, out Type? elementType )
        {
            elementType = null;

            if( !toCheck.IsGenericType )
                return false;

            if( !typeof(List<>).IsAssignableFrom( toCheck.GetGenericTypeDefinition() ) )
                return false;

            elementType = toCheck.GetGenericArguments()[ 0 ];

            return true;
        }
    }
}