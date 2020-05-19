using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public partial class TargetableProperty
    {
        private readonly StringComparison _keyComp;

        private TargetableProperty( PropertyInfo propertyInfo, StringComparison keyComp )
        {
            PropertyInfo = propertyInfo;
            _keyComp = keyComp;
        }

        public PropertyInfo PropertyInfo { get; }

        public bool IsCreateable { get; private set; }
        public bool IsDefined { get; private set; }
        public bool IsPubliclyReadWrite { get; private set; }
        public PropertyMultiplicity Multiplicity { get; private set; }

        public bool IsTargetable => ( IsCreateable || IsDefined )
                                    && IsPubliclyReadWrite
                                    && Multiplicity != PropertyMultiplicity.Unsupported;

        public IOption? BoundOption { get; set; }

        public List<PropertyInfo> PathElements { get; private set; }

        public string FullPath
        {
            get
            {
                var retVal = PathElements.Aggregate( new StringBuilder(), ( sb, pi ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( "." );

                    sb.Append( pi.Name );

                    return sb;
                }, sb =>
                {
                    if( sb.Length > 0 )
                        sb.Append( "." );

                    sb.Append( PropertyInfo.Name );

                    return sb.ToString();
                } );

                return retVal;
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
                return MappingResults.Unbound;
            }

            if( Multiplicity == PropertyMultiplicity.Unsupported )
            {
                logger?.Error<string>( "Property {0} has an unsupported Multiplicity", PropertyInfo.Name );
                return MappingResults.UnsupportedMultiplicity;
            }

            // see if our BoundOption's keys match a key in the parse results so we can retrieve a
            // specific IParseResult
            var parseResult = parseResults
                .FirstOrDefault( pr =>
                    BoundOption.Keys.Any( k => string.Equals( k, pr.Key, _keyComp ) ) );

            var retVal = MappingResults.Success;
            string optionKey;

            // start by setting the value we're going to set on our bound property to 
            // whatever default was specified for our BoundOption. if the stored
            // default value is null we create an empty collection
            object? defaultValue;

            if( BoundOption.DefaultValue != null )
                defaultValue = BoundOption.DefaultValue;
            else
                defaultValue = Multiplicity switch
                {
                    PropertyMultiplicity.Array => BoundOption.CreateEmptyArray( 0 ),
                    PropertyMultiplicity.List => BoundOption.CreateEmptyList(),
                    _ => null
                };

            var propValue = defaultValue;

            if( parseResult == null )
            {
                // set a return flag if there's no matching IParseResult and pick a default key
                // value (needed for displaying context-sensitive help)
                logger?.Error<string>( "No matching argument keys for property {0}", PropertyInfo.Name );

                if( BoundOption.IsRequired )
                    retVal |= MappingResults.MissingRequired;

                optionKey = BoundOption.Keys.First();
            }
            else
            {
                // store the option key that we matched on for later use in displaying context-sensitive help
                optionKey = parseResult.Key;

                // check to see if we have a valid number of parameters to convert
                if( parseResult.NumParameters >= BoundOption.MinParameters
                    && parseResult.NumParameters <= BoundOption.MaxParameters )
                {
                    // the particular Option conversion method we call depends on whether or not we're binding to 
                    // a collection/array or a single value
                    switch( Multiplicity )
                    {
                        case PropertyMultiplicity.Array:
                        case PropertyMultiplicity.List:
                            if( BoundOption.ConvertList( bindingTarget, parseResult, out var collectionResult ) !=
                                TextConversionResult.Okay )
                            {
                                logger?.Error<string, string>( "Couldn't parse {0} to property {1}",
                                    parseResult.ParametersToText(), PropertyInfo.Name );

                                // set a flag to show the error. Note that the default value is still 
                                // the value we'll use to set our target property
                                retVal |= MappingResults.ConversionFailed;
                            }
                            else
                            {
                                // if conversion succeeded, store the result, converting it to a
                                // simple array if necessary
                                if( Multiplicity == PropertyMultiplicity.Array )
                                {
                                    var tempArray = BoundOption.CreateEmptyArray( collectionResult.Count );

                                    for( var idx = 0; idx < collectionResult.Count; idx++ )
                                        tempArray.SetValue( collectionResult[ idx ], idx );

                                    propValue = tempArray;
                                }
                                else
                                {
                                    propValue = collectionResult;
                                }
                            }

                            break;

                        case PropertyMultiplicity.SingleValue:
                        case PropertyMultiplicity.String:
                            if( BoundOption.Convert( bindingTarget, parseResult, out var singleResult ) !=
                                TextConversionResult.Okay )
                            {
                                logger?.Error<string, string>( "Couldn't parse {0} to property {1}",
                                    parseResult.ParametersToText(), PropertyInfo.Name );

                                // set a flag to show the error. Note that the default value is still 
                                // the value we'll use to set our target property
                                retVal |= MappingResults.ConversionFailed;
                            }
                            else
                            {
                                propValue = singleResult;
                            }

                            break;
                    }
                }
                else
                {
                    if( parseResult.NumParameters < BoundOption.MinParameters )
                        retVal |= MappingResults.TooFewParameters;

                    if( parseResult.NumParameters > BoundOption.MaxParameters )
                        retVal |= MappingResults.TooManyParameters;
                }
            }

            if( propValue != null && !BoundOption.Validate( bindingTarget, optionKey, propValue ) )
            {
                // revert to our default value (which we presume is valid but don't actually know
                // or care)
                propValue = defaultValue;

                // set a flag to record the validation failure
                retVal |= MappingResults.ValidationFailed;
            }

            // navigate down to our immediate container,
            // initializing stuff as needed along the way...
            var container = bindingTarget.GetValue();

            for( var idx = 0; idx < PathElements.Count; idx++ )
            {
                var value = PathElements[ idx ].GetValue( container );

                if( idx < PathElements.Count - 1 )
                {
                    value ??= Activator.CreateInstance( PathElements[ idx ].PropertyType );

                    container = value!;
                }
            }

            // finally, set the target property's value
            PropertyInfo.SetValue( container, propValue );

            return retVal;
        }
    }
}