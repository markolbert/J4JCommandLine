using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class HelpOption : OptionBase
    {
        public HelpOption(
            IOptionCollection options,
            IJ4JLogger? logger
        )
            : base( OptionType.Help, new UntargetableType(), options, logger )
        {
            MaxParameters = 0;
        }

        public override MappingResults Convert( 
            IBindingTarget bindingTarget, 
            IParseResult parseResult, 
            ITargetableType targetType,
            out object? result )
        {
            result = null;

            return MappingResults.HelpRequested;
        }
    }
}