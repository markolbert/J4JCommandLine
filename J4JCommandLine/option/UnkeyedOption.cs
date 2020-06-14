﻿using System.Collections.Generic;

namespace J4JSoftware.CommandLine
{
    // defines an IOption which always returns null when used as the target of a parsing
    // operation. Used to capture error information about an invalid parsing.
    public class UnkeyedOption : Option
    {
        public UnkeyedOption(OptionCollection options )
            : base( OptionType.Unkeyed, new UntargetableType(), options )
        {
        }

        public override MappingResults Convert(
            IBindingTarget bindingTarget,
            IParseResult parseResult,
            ITargetableType targetType,
            out object? result)
        {
            result = null;

            return MappingResults.Unbound;
        }
    }
}