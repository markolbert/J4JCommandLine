using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines an IOption which always returns null when used as the target of a parsing
    // operation. Used to capture error information about an invalid parsing.
    public class UntargetedOption : Option
    {
        public UntargetedOption(OptionCollection options )
            : base( OptionType.Null, new UntargetableType(), options )
        {
        }

        public override MappingResult Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            ITargetableType targetType,
            out object? result)
        {
            result = null;

            return MappingResult.Unbound;
        }
    }
}