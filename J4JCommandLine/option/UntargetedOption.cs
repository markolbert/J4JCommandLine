namespace J4JSoftware.CommandLine.Deprecated
{
    // defines an IOption which always returns null when used as the target of a parsing
    // operation. Used to capture error information about an invalid parsing.
    public class UntargetedOption : Option
    {
        public UntargetedOption(OptionCollection options, CommandLineLogger logger )
            : base( OptionType.Null, new UntargetableType(), options, logger )
        {
        }

        public override object? Convert( IAllocation allocation, ITargetableType targetType )
        {
            Logger.LogError( ProcessingPhase.Parsing, 
                "Option is not bound to a property",
                option : this );

            return null;
        }
    }
}