using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class DefaultTypeInitializer : TypeInitializerBase
    {
        public DefaultTypeInitializer( IJ4JLogger logger ) 
            : base( logger )
        {
        }

        protected override bool SupportedProperty( PropertyInfo propInfo )
        {
            if( IsValueType( propInfo.PropertyType ) )
                return true;

            if( HasParameterlessConstructor( propInfo.PropertyType ) )
                return true;

            if( IsArray( propInfo.PropertyType, out var arrayElementType ) )
            {
                if( IsValueType( arrayElementType! ) || HasParameterlessConstructor( arrayElementType! ) )
                    return true;
            }

            if (IsList(propInfo.PropertyType, out var listElementType))
            {
                if (IsValueType(listElementType!) || HasParameterlessConstructor(listElementType!))
                    return true;
            }

            return false;
        }
    }
}