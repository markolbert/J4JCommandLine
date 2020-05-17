using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TextToText : TextConverter<string>
    {
        public override bool Convert( string value, out string result )
        {
            result = value;

            return true;
        }
    }
}