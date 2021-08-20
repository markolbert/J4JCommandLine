using System;
using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine
{
    [CommandLineCustomization(Customization.BuiltIn, Int32.MinValue)]
    public class BuiltInOptionsGenerator : OptionsGenerator
    {
        public BuiltInOptionsGenerator(
            IJ4JLogger? logger
        )
            : base( logger )
        {
        }
    }
}