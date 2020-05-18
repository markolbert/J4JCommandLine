using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public partial class TargetableProperty
    {
        public static TargetableProperty Create(
            PropertyInfo propInfo,
            object? container,
            StringComparison keyComp,
            Stack<PropertyInfo> pathToContainer,
            IJ4JLogger? logger = null )
        {
            PropertyMultiplicity multiplicity;
            Type relevantType = propInfo.PropertyType;

            if( propInfo.PropertyType.IsArray )
            {
                // we want to ensure we can target/create the underlying element type
                var elemType = propInfo.PropertyType.GetElementType();

                if( elemType == null )
                {
                    logger?.Error<string>( "Cannot determine element type for array {0}", propInfo.Name );
                    multiplicity = PropertyMultiplicity.Unsupported;
                }
                else multiplicity = PropertyMultiplicity.Array;
            }
            else
            {
                // see if the property type is a List<>
                if( typeof(ICollection).IsAssignableFrom( propInfo.PropertyType ) )
                {
                    if( propInfo.PropertyType.IsGenericType )
                    {
                        if( propInfo.PropertyType.GenericTypeArguments.Length == 1 )
                        {
                            multiplicity = PropertyMultiplicity.List;
                            relevantType = propInfo.PropertyType.GenericTypeArguments[ 0 ];
                        }
                        else
                        {
                            logger?.Error<string>( "ICollection<> {0} has more than one generic parameter",
                                propInfo.Name );
                            multiplicity = PropertyMultiplicity.Unsupported;
                        }
                    }
                    else
                    {
                        logger?.Error<string>( "ICollection {0} is not generic", propInfo.Name );
                        multiplicity = PropertyMultiplicity.Unsupported;
                    }
                }
                else
                {
                    var numIndexParams = propInfo.GetMethod?.GetParameters().Length;

                    if( numIndexParams == 0 )
                        multiplicity = typeof(string).IsAssignableFrom( propInfo.PropertyType )
                            ? PropertyMultiplicity.String
                            : PropertyMultiplicity.SingleValue;
                    else
                    {
                        logger?.Error<string>( "Property {0} is indexed but in an unsupported way", propInfo.Name );
                        multiplicity = PropertyMultiplicity.Unsupported;
                    }
                }
            }

            var retVal = new TargetableProperty( propInfo, keyComp )
            {
                Multiplicity = multiplicity,
                IsCreateable = relevantType.HasPublicParameterlessConstructor(),
                IsDefined = propInfo.GetValue( container ) != null,
                IsPubliclyReadWrite = propInfo.IsPublicReadWrite( logger ),
                PathElements = pathToContainer.ToList()
            };

            return retVal;
        }
    }
}