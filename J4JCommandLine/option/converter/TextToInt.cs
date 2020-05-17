using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TextToInt : TextConverter<int>
    {
        public override bool Convert( string value, out int result )
        {
            if (int.TryParse(value, out var innerResult))
            {
                result = innerResult;
                return true;
            }

            result = default(int);

            return false;
        }
    }
}