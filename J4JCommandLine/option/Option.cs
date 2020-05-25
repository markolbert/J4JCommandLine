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
            ITargetableType targetableType,
            IJ4JLogger? logger = null
        )
            : base( OptionType.Mappable, targetableType, options, logger )
        {
            _converter = targetableType.Converter!;
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
                case Multiplicity.Array:
                    retVal = ConvertArray( bindingTarget, parseResult, out var arrayResult );
                    result = arrayResult;

                    break;

                case Multiplicity.List:
                    retVal = ConvertList( bindingTarget, parseResult, out var listResult );
                    result = listResult;
                    break;

                case Multiplicity.SimpleValue:
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
            if( !ValidParameterCount( parseResult, out var paramResult ) )
            {
                result = null;
                return paramResult;
            }

            // handle boolean flag parameters which don't have a parameter
            var text = TargetableType.SupportedType == typeof( bool )
                       && parseResult.NumParameters == 0
                ? "true"
                : parseResult.Parameters[ 0 ];

            result = Convert( bindingTarget, parseResult.Key, text );

            if( result != null )
                return MappingResults.Success;

            bindingTarget.AddError( parseResult.Key, $"Couldn't convert '{text}' to {TargetableType}" );

            return MappingResults.ConversionFailed;
        }

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
    }
}