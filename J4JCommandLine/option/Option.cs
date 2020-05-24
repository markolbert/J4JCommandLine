using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Option : OptionBase
    {
        private readonly ITextConverter _converter;

        public Option(
            IOptionCollection options,
            ITextConverter converter,
            ITargetableType targetableType,
            IJ4JLogger? logger = null
        )
            : base( OptionType.Mappable, targetableType, options, logger )
        {
            _converter = converter;
        }

        public override MappingResults Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            ITargetableType targetType,
            out object? result )
        {
            var retVal = MappingResults.Success;

            switch( targetType.Multiplicity )
            {
                case PropertyMultiplicity.Array:
                    retVal = ConvertArray( bindingTarget, parseResult, out var arrayResult );
                    result = arrayResult;

                    break;

                case PropertyMultiplicity.List:
                    retVal = ConvertList( bindingTarget, parseResult, out var listResult );
                    result = listResult;
                    break;

                case PropertyMultiplicity.SingleValue:
                    retVal = ConvertSingleValue( bindingTarget, parseResult, out var singleResult );
                    result = singleResult;
                    break;

                default:
                    result = null;
                    retVal = MappingResults.UnsupportedMultiplicity;
                    break;
            }

            return retVal;
        }

        private object? Convert(IBindingTarget bindingTarget, string key, string text)
        {
            if (_converter.Convert(text, out var innerResult))
                return innerResult;

            bindingTarget.AddError(
                key,
                $"Couldn't convert '{text}' to {_converter.SupportedType}");

            return null;
        }

        private MappingResults ConvertSingleValue( IBindingTarget bindingTarget, IParseResult parseResult,
            out object? result )
        {
            if( parseResult.NumParameters == 1 )
            {
                bindingTarget.AddError( parseResult.Key,
                    $"Incorrect number of parameters. Expected 1, got {parseResult.NumParameters}" );

                result = null;
                return MappingResults.ConversionFailed;
            }

            result = Convert( bindingTarget, parseResult.Key, parseResult.Parameters[ 0 ] );

            if( result != null )
                return MappingResults.Success;

            bindingTarget.AddError( parseResult.Key,
                $"Couldn't convert '{parseResult.Parameters[ 0 ]}' to {TargetableType}" );

            return MappingResults.ConversionFailed;
        }

        //public override IList CreateEmptyList()
        //{
        //    // create a list of the Type we're converting to
        //    var listType = typeof(List<>);
        //    var genericListType = listType.MakeGenericType( _converter.SupportedType );

        //    return ( Activator.CreateInstance( genericListType ) as IList )!;
        //}

        //public override Array CreateEmptyArray( int capacity )
        //{
        //    capacity = capacity < 0 ? 0 : capacity;

        //    return Array.CreateInstance( SupportedType, capacity );
        //}
        private MappingResults ConvertArray( IBindingTarget bindingTarget, IParseResult parseResult, out Array? result )
        {
            if( !ValidParameterCount( parseResult, out var paramResult ) )
            {
                result = null;
                return paramResult;
            }

            result = Array.CreateInstance( TargetableType.SupportedType, parseResult.NumParameters );

            var allOkay = true;

            for( var idx = 0; idx < parseResult.NumParameters; idx++ )
            {
                var item = Convert( bindingTarget, parseResult.Key, parseResult.Parameters[ idx ] );

                if( item == null )
                {
                    bindingTarget.AddError(
                        parseResult.Key,
                        $"Couldn't convert '{parseResult.Parameters[ idx ]}' to {_converter.SupportedType}" );

                    allOkay = false;

                    result = null;
                    break;
                }
                else result.SetValue( item, idx );
            }

            return allOkay ? MappingResults.Success : MappingResults.ConversionFailed;
        }

        private MappingResults ConvertList( IBindingTarget bindingTarget, IParseResult parseResult, out IList? result )
        {
            if (!ValidParameterCount(parseResult, out var paramResult))
            {
                result = null;
                return paramResult;
            }

            // create a list of the Type we're converting to
            var listType = typeof(List<>);
            var genericListType = listType.MakeGenericType( _converter.SupportedType );

            result = ( Activator.CreateInstance( genericListType ) as IList )!;

            var allOkay = true;

            for( var idx = 0; idx < parseResult.NumParameters; idx++ )
            {
                var item = Convert( bindingTarget, parseResult.Key, parseResult.Parameters[ idx ] );

                if( item == null )
                {
                    bindingTarget.AddError(
                        parseResult.Key,
                        $"Couldn't convert '{parseResult.Parameters[idx]}' to {_converter.SupportedType}");

                    allOkay = false;

                    result = null;
                    break;
                }
                else result.Add( item );
            }

            return allOkay ? MappingResults.Success : MappingResults.ConversionFailed;
        }

        private bool ValidParameterCount( IParseResult parseResult, out MappingResults result )
        {
            if( parseResult.NumParameters < MinParameters )
            {
                Logger?.Error<int, int>( "Expected {0} parameters, got {1}", MinParameters, parseResult.NumParameters );
                result = MappingResults.TooFewParameters;

                return false;
            }

            if (parseResult.NumParameters > MaxParameters)
            {
                Logger?.Error<int, int>("Expected {0} parameters, got {1}", MinParameters, parseResult.NumParameters);
                result = MappingResults.TooManyParameters;

                return false;
            }

            result = MappingResults.Success;

            return true;
        }
    }
}