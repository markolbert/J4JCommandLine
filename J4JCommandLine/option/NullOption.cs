using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class NullOption : OptionBase
    {
        public NullOption(
            IOptionCollection options,
            IJ4JLogger? logger )
            : base( OptionType.Null, typeof(object), options, logger )
        {
        }
    }
}