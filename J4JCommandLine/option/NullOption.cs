using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class NullOption : OptionBase
    {
        public NullOption(
            IOptionCollection options,
            IJ4JLogger? logger )
            : base( OptionType.Null, new UntargetableType(), options, logger )
        {
        }

        public override MappingResults Convert( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            ITargetableType targetType,
            out object? result )
        {
            result = null;

            return MappingResults.Unbound;
        }
    }
}