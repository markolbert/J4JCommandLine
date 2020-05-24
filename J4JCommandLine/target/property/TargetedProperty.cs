using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public partial class TargetedProperty
    {
        private readonly StringComparison _keyComp;

        //private List<PropertyInfo> _pathElements = new List<PropertyInfo>();
        //private string? _fullPath = null;

        public TargetedProperty( 
            PropertyInfo propertyInfo, 
            object? container,
            TargetedProperty? parent,
            ITargetableTypeFactory targetableTypeFactory, 
            StringComparison keyComp,
            IJ4JLogger? logger)
        {
            _keyComp = keyComp;

            PropertyInfo = propertyInfo;
            Parent = parent;
            TargetableType = targetableTypeFactory.Create( PropertyInfo.PropertyType );
            IsPubliclyReadWrite = PropertyInfo.IsPublicReadWrite( logger );
            IsPreAssigned = PropertyInfo.PropertyType.IsValueType || PropertyInfo.GetValue( container ) != null;
        }

        public TargetedProperty? Parent { get; }

        public List<TargetedProperty> PropertyPath
        {
            get
            {
                if( Parent == null )
                    return new List<TargetedProperty> { this };

                var retVal = Parent.PropertyPath;
                retVal.Add( this );

                return retVal;
            }
        }

        public PropertyInfo PropertyInfo { get; }
        public ITargetableType TargetableType { get; }
        public string Name => PropertyInfo.Name;
        public bool IsCreateable => TargetableType.IsCreatable;
        public bool IsPreAssigned { get; }
        public bool IsPubliclyReadWrite { get; private set; }
        public PropertyMultiplicity Multiplicity => TargetableType.Multiplicity;

        public bool IsTargetable => ( IsCreateable || IsPreAssigned )
                                    && IsPubliclyReadWrite
                                    && Multiplicity != PropertyMultiplicity.Unsupported;

        public IOption? BoundOption { get; set; }

        public object? GetContainer( IBindingTarget bindingTarget )
        {
            var propPath = PropertyPath.ToList();
            propPath.Reverse();

            object? retVal = bindingTarget.GetValue();

            // walk through the parent containers grabbing values,
            // creating the ones that are undefined
            foreach( var targeted in propPath.SkipLast(1) )
            {
                var newContainer = targeted.PropertyInfo.GetValue( retVal );

                // if the property doesn't have a value, create it
                // if possible
                if( newContainer != null ) 
                    continue;
                
                newContainer = TargetableType.Create();

                targeted.PropertyInfo.SetValue( retVal, newContainer );

                retVal = newContainer;

                if( retVal == null )
                    return null;
            }

            return retVal;
        }

        //public List<PropertyInfo> PathElements
        //{
        //    get => _pathElements;

        //    private set
        //    {
        //        _pathElements = value;
        //        _fullPath = null;
        //    }
        //}

        public string FullPath
        {
            get
            {
                return Parent == null ? PropertyInfo.Name : $"{Parent.FullPath}.{Name}";

                //if( _fullPath == null )
                //    _fullPath = PathElements.Aggregate( new StringBuilder(), ( sb, pi ) =>
                //    {
                //        if( sb.Length > 0 )
                //            sb.Append( "." );

                //        sb.Append( pi.Name );

                //        return sb;
                //    }, sb =>
                //    {
                //        if( sb.Length > 0 )
                //            sb.Append( "." );

                //        sb.Append( Name );

                //        return sb.ToString();
                //    } );

                //return _fullPath;
            }
        }

        public MappingResults MapParseResult(
            IBindingTarget bindingTarget,
            ParseResults parseResults,
            IJ4JLogger? logger = null )
        {
            // validate parameters and state
            if( BoundOption == null )
            {
                logger?.Error<string>( "Trying to map parsing results to unbound property {0}", PropertyInfo.Name );
                bindingTarget.AddError("?", $"Property '{PropertyInfo.Name}' is unbound");

                return MappingResults.Unbound;
            }

            // see if our BoundOption's keys match a key in the parse results so we can retrieve a
            // specific IParseResult
            var parseResult = parseResults
                .FirstOrDefault(pr =>
                    BoundOption.Keys.Any(k => string.Equals(k, pr.Key, _keyComp)));

            // store the option key that we matched on for later use in displaying context-sensitive help
            var optionKey = parseResult == null ? BoundOption.Keys.First() : parseResult.Key;

            if ( Multiplicity == PropertyMultiplicity.Unsupported )
            {
                logger?.Error<string>( "Property {0} has an unsupported Multiplicity", PropertyInfo.Name );
                bindingTarget.AddError(optionKey, $"Property '{PropertyInfo.Name}' has an unsupported Multiplicity");

                return MappingResults.UnsupportedMultiplicity;
            }

            if( !IsPreAssigned && !IsCreateable )
            {
                logger?.Error<string>("Property {0} was not pre-assigned and is not creatable", PropertyInfo.Name);
                bindingTarget.AddError(optionKey, $"Property '{PropertyInfo.Name}' was not pre-assigned and is not creatable");

                return MappingResults.NotDefinedOrCreatable;
            }

            if (!IsPubliclyReadWrite)
            {
                logger?.Error<string>("Property {0} is not publicly readable/writeable", PropertyInfo.Name);
                bindingTarget.AddError(optionKey, $"Property '{PropertyInfo.Name}' is not publicly readable/writeable");

                return MappingResults.NotPublicReadWrite;
            }

            var retVal = MappingResults.Success;

            // start by setting the value we're going to set on our bound property to 
            // whatever default was specified for our BoundOption.
            var propValue = BoundOption.DefaultValue;

            if( parseResult == null )
            {
                // set a return flag if there's no matching IParseResult
                logger?.Error<string>( "No matching argument keys for property {0}", PropertyInfo.Name );

                // if the option isn't required we'll just use the previously-determined default value
                if ( BoundOption.IsRequired )
                {
                    bindingTarget.AddError(optionKey, $"Missing required option '{optionKey}'");
                    retVal |= MappingResults.MissingRequired;
                }
            }
            else
            {
                retVal = BoundOption.Convert( bindingTarget, parseResult, TargetableType, out var convResult );
                //// check to see if we have a valid number of parameters to convert
                //if( parseResult.NumParameters >= BoundOption.MinParameters
                //    && parseResult.NumParameters <= BoundOption.MaxParameters )
                //{
                //    // if we're creating an array, size it to hold the observed number of parameters
                //    var createArgs = new object[] { parseResult.NumParameters };

                //    // the particular Option conversion method we call depends on whether or not we're binding to 
                //    // a collection/array or a single value
                //    switch( Multiplicity )
                //    {
                //        case PropertyMultiplicity.Array:
                //        case PropertyMultiplicity.List:
                //            if( BoundOption.ConvertList( bindingTarget, parseResult, out var collectionResult ) !=
                //                TextConversionResult.Okay )
                //            {
                //                logger?.Error<string, string>( "Couldn't parse {0} to property {1}",
                //                    parseResult.ParametersToText(), PropertyInfo.Name );

                //                // set a flag to show the error. Note that the default value is still 
                //                // the value we'll use to set our target property
                //                retVal |= MappingResults.ConversionFailed;
                //            }
                //            else
                //            {
                //                // if conversion succeeded, store the result, converting it to a
                //                // simple array if necessary
                //                if( Multiplicity == PropertyMultiplicity.Array )
                //                {
                //                    var tempArray = BoundOption.CreateEmptyArray( collectionResult.Count );

                //                    for( var idx = 0; idx < collectionResult.Count; idx++ )
                //                        tempArray.SetValue( collectionResult[ idx ], idx );

                //                    propValue = tempArray;
                //                }
                //                else
                //                {
                //                    propValue = collectionResult;
                //                }
                //            }

                //            break;

                //        case PropertyMultiplicity.SingleValue:
                //        case PropertyMultiplicity.String:
                //            if( BoundOption.Convert( bindingTarget, parseResult, out var singleResult ) !=
                //                TextConversionResult.Okay )
                //            {
                //                logger?.Error<string, string>( "Couldn't parse {0} to property {1}",
                //                    parseResult.ParametersToText(), PropertyInfo.Name );

                //                // set a flag to show the error. Note that the default value is still 
                //                // the value we'll use to set our target property
                //                retVal |= MappingResults.ConversionFailed;
                //            }
                //            else
                //            {
                //                propValue = singleResult;
                //            }

                //            break;
                //    }
                //}
                //else
                //{
                //    if( parseResult.NumParameters < BoundOption.MinParameters )
                //        retVal |= MappingResults.TooFewParameters;

                //    if( parseResult.NumParameters > BoundOption.MaxParameters )
                //        retVal |= MappingResults.TooManyParameters;
                //}
            }

            if( propValue != null && !BoundOption.Validate( bindingTarget, optionKey, propValue ) )
            {
                // revert to our default value (which we presume is valid but don't actually know
                // or care)
                propValue = BoundOption.DefaultValue;

                // set a flag to record the validation failure
                retVal |= MappingResults.ValidationFailed;
            }

            // finally, set the target property's value
            PropertyInfo.SetValue( GetContainer( bindingTarget ), propValue );

            return retVal;
        }
    }
}