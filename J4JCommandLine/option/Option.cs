using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Option : OptionBase
    {
        private readonly ITextConverter _converter;

        public Option( 
            IOptionCollection options,
            ITextConverter converter,
            IJ4JLogger? logger = null 
        )
        : base(converter.SupportedType, options, logger)
        {
            _converter = converter;
        }

        public override TextConversionResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out object result )
        {
            if (parseResult.NumParameters != 1)
            {
                result = DefaultValue;

                bindingTarget.AddError(parseResult.Key, $"Incorrect number of parameters. Expected 1, got {parseResult.NumParameters}");
                return TextConversionResult.FailedConversion;
            }

            if( _converter.Convert( parseResult.Parameters[ 0 ], out var innerResult ) )
            {
                result = innerResult;
                return TextConversionResult.Okay;
            }

            result = DefaultValue;

            bindingTarget.AddError(
                parseResult.Key,
                $"Couldn't convert '{parseResult.Parameters[0]}' to {_converter.SupportedType}");

            return TextConversionResult.FailedConversion;
        }

        public override IList CreateEmptyList()
        {
            // create a list of the Type we're converting to
            var listType = typeof(List<>);
            var genericListType = listType.MakeGenericType(_converter.SupportedType);

            return (Activator.CreateInstance(genericListType) as IList)!;
        }

        public override TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out IList result )
        {
            // create a list of the Type we're converting to
            result = CreateEmptyList();

            var allOkay = true;

            foreach( var parameter in parseResult.Parameters )
            {
                if( _converter.Convert( parameter, out var innerResult ) )
                    result.Add( innerResult );
                else
                {
                    bindingTarget.AddError(
                        parseResult.Key,
                        $"Couldn't convert '{parameter}' to {_converter.SupportedType}" );

                    allOkay = false;
                }
            }

            return allOkay ? TextConversionResult.Okay : TextConversionResult.FailedConversion;
        }
    }
}