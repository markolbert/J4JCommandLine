using System;
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

        public override TextConversionResult ConvertList(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            out List<object> result )
        {
            result = new List<object>();

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