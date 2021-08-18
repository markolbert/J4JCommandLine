using System;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    public class BuiltInOptionsGenerator : OptionsGenerator
    {
        public BuiltInOptionsGenerator(
            IJ4JLogger? logger
        )
            : base( Customization.BuiltIn, Int32.MinValue, logger )
        {
        }
    }
}