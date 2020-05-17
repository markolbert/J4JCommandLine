using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TextToDouble : TextConverter<double>
    {
        public override bool Convert( string value, out double result )
        {
            if (double.TryParse(value, out var innerResult))
            {
                result = innerResult;
                return true;
            }

            result = default(double);

            return false;
        }
    }
}