using System;
using System.Collections;
using System.Collections.Generic;

namespace J4JSoftware.CommandLine.Deprecated
{
    public class MappableOption : Option
    {
        private readonly ITextConverter _converter;

        public MappableOption(
            OptionCollection options,
            ITargetableType targetableType,
            CommandLineLogger logger,
            bool isKeyed
        )
            : base( isKeyed ? OptionType.Keyed : OptionType.Unkeyed, targetableType, options, logger )
        {
            _converter = targetableType.Converter!;
        }

        // the method called to convert the parsing results for a particular command
        // line key to a option value. Return values other than MappingResult.Success
        // indicate one or more problems were encountered in the conversion and validation
        // process
        public override object? Convert(
            IAllocation allocation,
            ITargetableType targetType )
        {
            return targetType.Multiplicity switch
            {
                PropertyMultiplicity.Array => ConvertToArray( allocation ),
                PropertyMultiplicity.List => ConvertToList( allocation ),
                PropertyMultiplicity.SimpleValue => ConvertToSimpleValue( allocation ),
                _ => null
            };
        }

        // attempts to convert a text value using the defined ITextConverter property
        private object? Convert(string key, string text)
        {
            if (_converter.Convert(text, out var innerResult))
                return innerResult;

            Logger.LogError(
                ProcessingPhase.Parsing,
                $"Couldn't convert '{text}' to {_converter.SupportedType}",
                option : this );

            return null;
        }

        // checks to see if the provided IAllocation contains an allowable number of parameters and, if so,
        // attempts to convert them to a simple value (i.e., a single object, an IValueType, a string)
        private object? ConvertToSimpleValue( IAllocation allocation )
        {
            if( !ValidParameterCount( allocation ) )
                return null;

            // handle boolean flag parameters which don't have a parameter
            var text = OptionStyle == OptionStyle.Switch && allocation.NumParameters == 0
                ? "true"
                : allocation.Parameters[ 0 ];

            var retVal = Convert( allocation.Key!, text );

            if( retVal != null )
                return retVal;

            Logger.LogError( ProcessingPhase.Parsing, $"Couldn't convert '{text}' to {TargetableType}", option : this );

            return null;
        }

        // checks to see if the provided IAllocation contains an allowable number of parameters and, if so,
        // attempts to convert them to an array of simple values (i.e., a single object, an IValueType, a string)
        private Array? ConvertToArray( IAllocation allocation )
        {
            if( !ValidParameterCount( allocation ) )
                return null;

            var retVal = Array.CreateInstance( TargetableType.SupportedType, allocation.NumParameters );

            var allOkay = true;

            for( var idx = 0; idx < allocation.NumParameters; idx++ )
            {
                var item = Convert( allocation.Key!, allocation.Parameters[ idx ] );

                if( item == null )
                {
                    Logger.LogError(
                        ProcessingPhase.Parsing,
                        $"Couldn't convert '{allocation.Parameters[ idx ]}' to {_converter.SupportedType}",
                        option: this);

                    allOkay = false;
                    break;
                }
                else retVal.SetValue( item, idx );
            }

            return allOkay ? retVal : null;
        }

        // checks to see if the provided IAllocation contains an allowable number of parameters and, if so,
        // attempts to convert them to a generic list of simple values (i.e., a single object, an IValueType, a string)
        private IList? ConvertToList( IAllocation allocation )
        {
            if( !ValidParameterCount( allocation ) )
                return null;

            // create a list of the Type we're converting to
            var listType = typeof(List<>);
            var genericListType = listType.MakeGenericType( _converter.SupportedType );

            var retVal = ( Activator.CreateInstance( genericListType ) as IList )!;

            var allOkay = true;

            for( var idx = 0; idx < allocation.NumParameters; idx++ )
            {
                var item = Convert( allocation.Key, allocation.Parameters[ idx ] );

                if( item == null )
                {
                    Logger.LogError(
                        ProcessingPhase.Parsing,
                        $"Couldn't convert '{allocation.Parameters[ idx ]}' to {_converter.SupportedType}",
                        option : this );

                    allOkay = false;
                    break;
                }
                else retVal.Add( item );
            }

            return allOkay ? retVal : null;
        }
    }
}