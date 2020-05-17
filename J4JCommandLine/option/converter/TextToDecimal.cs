using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TextToDecimal : TextConverter<decimal>
    {
        public override bool Convert( string value, out decimal result )
        {
            if (decimal.TryParse(value, out var innerResult))
            {
                result = innerResult;
                return true;
            }

            result = default(decimal);

            return false;
        }
    }
}