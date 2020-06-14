using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    public class MappableOption : Option
    {
        private readonly ITextConverter _converter;

        public MappableOption(
            OptionCollection options,
            ITargetableType targetableType,
            bool isKeyed
        )
            : base( isKeyed ? OptionType.Keyed : OptionType.Unkeyed, targetableType, options )
        {
            _converter = targetableType.Converter!;
        }

        // the method called to convert the parsing results for a particular command
        // line key to a option value. Return values other than MappingResults.Success
        // indicate one or more problems were encountered in the conversion and validation
        // process
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
                    retVal = ConvertToArray( bindingTarget, parseResult, out var arrayResult );
                    result = arrayResult;

                    break;

                case Multiplicity.List:
                    retVal = ConvertToList( bindingTarget, parseResult, out var listResult);
                    result = listResult;

                    break;

                case Multiplicity.SimpleValue:
                    retVal = ConvertToSimpleValue( bindingTarget, parseResult, out var singleResult);
                    result = singleResult;

                    break;

                default:
                    result = null;
                    retVal = MappingResults.UnsupportedMultiplicity;

                    break;
            }

            return retVal;
        }

        // attempts to convert a text value using the defined ITextConverter property
        private object? Convert(IBindingTarget bindingTarget, string key, string text)
        {
            if (_converter.Convert(text, out var innerResult))
                return innerResult;

            bindingTarget.AddError(
                key,
                $"Couldn't convert '{text}' to {_converter.SupportedType}");

            return null;
        }

        // checks to see if the provided IParseResult contains an allowable number of parameters and, if so,
        // attempts to convert them to a simple value (i.e., a single object, an IValueType, a string)
        private MappingResults ConvertToSimpleValue( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult,
            out object? result )
        {
            if ( !ValidParameterCount(bindingTarget, parseResult, out var paramResult ) )
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

        // checks to see if the provided IParseResult contains an allowable number of parameters and, if so,
        // attempts to convert them to an array of simple values (i.e., a single object, an IValueType, a string)
        private MappingResults ConvertToArray( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            out Array? result )
        {
            if( !ValidParameterCount( bindingTarget, parseResult, out var paramResult ) )
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

        // checks to see if the provided IParseResult contains an allowable number of parameters and, if so,
        // attempts to convert them to a generic list of simple values (i.e., a single object, an IValueType, a string)
        private MappingResults ConvertToList( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            out IList? result )
        {
            if (!ValidParameterCount( bindingTarget, parseResult, out var paramResult ))
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