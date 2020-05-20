using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class HelpOption : OptionBase
    {
        public HelpOption(
            IOptionCollection options,
            IJ4JLogger? logger
        )
            : base( OptionType.Help, typeof(object), options, logger )
        {
            MaxParameters = 0;
        }
    }
}