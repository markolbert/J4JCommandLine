using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public partial class TargetedProperty
    {
        //public static TargetedProperty Create(
        //    PropertyInfo propInfo,
        //    object? container,
        //    StringComparison keyComp,
        //    Stack<PropertyInfo> pathToContainer,
        //    IJ4JLogger? logger = null )
        //{
        //    PropertyMultiplicity multiplicity;
        //    var relevantType = propInfo.PropertyType;

        //    if( propInfo.PropertyType.IsArray )
        //    {
        //        // we want to ensure we can target/create the underlying element type
        //        relevantType = propInfo.PropertyType.GetElementType();

        //        if( relevantType == null )
        //        {
        //            logger?.Error<string>( "Cannot determine element type for array {0}", propInfo.Name );
        //            multiplicity = PropertyMultiplicity.Unsupported;
        //        }
        //        else
        //        {
        //            multiplicity = PropertyMultiplicity.Array;
        //        }
        //    }
        //    else
        //    {
        //        // see if the property type is a List<>
        //        if( typeof(ICollection).IsAssignableFrom( propInfo.PropertyType ) )
        //        {
        //            if( propInfo.PropertyType.IsGenericType )
        //            {
        //                if( propInfo.PropertyType.GenericTypeArguments.Length == 1 )
        //                {
        //                    multiplicity = PropertyMultiplicity.List;
        //                    relevantType = propInfo.PropertyType.GenericTypeArguments[ 0 ];
        //                }
        //                else
        //                {
        //                    logger?.Error<string>( "ICollection<> {0} has more than one generic parameter",
        //                        propInfo.Name );
        //                    multiplicity = PropertyMultiplicity.Unsupported;
        //                }
        //            }
        //            else
        //            {
        //                logger?.Error<string>( "ICollection {0} is not generic", propInfo.Name );
        //                multiplicity = PropertyMultiplicity.Unsupported;
        //            }
        //        }
        //        else
        //        {
        //            var numIndexParams = propInfo.GetMethod?.GetParameters().Length;

        //            if( numIndexParams == 0 )
        //            {
        //                multiplicity = typeof(string).IsAssignableFrom( propInfo.PropertyType )
        //                    ? PropertyMultiplicity.String
        //                    : PropertyMultiplicity.SingleValue;
        //            }
        //            else
        //            {
        //                logger?.Error<string>( "Property {0} is indexed but in an unsupported way", propInfo.Name );
        //                multiplicity = PropertyMultiplicity.Unsupported;
        //            }
        //        }
        //    }

        //    var retVal = new TargetedProperty( propInfo, keyComp )
        //    {
        //        Multiplicity = multiplicity,
        //        IsCreateable = relevantType!.HasPublicParameterlessConstructor(),
        //        IsDefined = propInfo.GetValue( container ) != null,
        //        IsPubliclyReadWrite = propInfo.IsPublicReadWrite( logger ),
        //        PathElements = pathToContainer.ToList()
        //    };

        //    return retVal;
        //}

        //// create a TargetedProperty based on the collection of PropertyInfo objects that
        //// define the  "path", through various declaring types, to the property of interest.
        //// Those PropertyInfo objects must be ordered from "root" to "leaf" (i.e., starting with
        //// the ultimate parent property and ending with the property for which we are creating
        //// a TargetedProperty).
        //public static TargetedProperty Create(
        //    List<PropertyInfo> properties,
        //    object? container,
        //    StringComparison keyComp,
        //    IJ4JLogger? logger = null )
        //{
        //    PropertyMultiplicity multiplicity;
        //    var targetProp = properties.Last();
        //    var relevantType = properties.Last().PropertyType;

        //    if( relevantType.IsArray )
        //    {
        //        // we want to ensure we can target/create the underlying element type
        //        relevantType = relevantType.GetElementType()!;

        //        if( relevantType == null )
        //        {
        //            logger?.Error<string>( "Cannot determine element type for array {0}", targetProp.Name );
        //            multiplicity = PropertyMultiplicity.Unsupported;
        //        }
        //        else
        //        {
        //            multiplicity = PropertyMultiplicity.Array;
        //        }
        //    }
        //    else
        //    {
        //        // see if the property type is a List<>
        //        if( typeof(ICollection).IsAssignableFrom( relevantType ) )
        //        {
        //            if( relevantType.IsGenericType )
        //            {
        //                if( relevantType.GenericTypeArguments.Length == 1 )
        //                {
        //                    multiplicity = PropertyMultiplicity.List;
        //                    relevantType = relevantType.GenericTypeArguments[ 0 ];
        //                }
        //                else
        //                {
        //                    logger?.Error<string>(
        //                        "ICollection<> {0} has more than one generic parameter",
        //                        targetProp.Name );

        //                    multiplicity = PropertyMultiplicity.Unsupported;
        //                }
        //            }
        //            else
        //            {
        //                logger?.Error<string>( "ICollection {0} is not generic", targetProp.Name );
        //                multiplicity = PropertyMultiplicity.Unsupported;
        //            }
        //        }
        //        else
        //        {
        //            var numIndexParams = targetProp.GetMethod?.GetParameters().Length;

        //            if( numIndexParams == 0 )
        //            {
        //                multiplicity = typeof(string).IsAssignableFrom( relevantType )
        //                    ? PropertyMultiplicity.String
        //                    : PropertyMultiplicity.SingleValue;
        //            }
        //            else
        //            {
        //                logger?.Error<string>(
        //                    "Property {0} is indexed but in an unsupported way",
        //                    targetProp.Name );

        //                multiplicity = PropertyMultiplicity.Unsupported;
        //            }
        //        }
        //    }

        //    // walk down the property path to see if all the 
        //    // intermediate containers are defined
        //    var isDefined = true;

        //    foreach( var propertyInfo in properties )
        //    {
        //        container = propertyInfo.GetValue( container );

        //        if( container == null )
        //        {
        //            isDefined = false;
        //            break;
        //        }
        //    }

        //    var retVal = new TargetedProperty( targetProp, keyComp )
        //    {
        //        Multiplicity = multiplicity,
        //        IsCreateable = properties.HasPublicParameterlessConstructors(),
        //        IsDefined = isDefined,
        //        IsPubliclyReadWrite = targetProp.IsPublicReadWrite( logger ),
        //        PathElements = properties
        //    };

        //    return retVal;
        //}
    }
}