using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TextToFloat : TextConverter<float>
    {
        public override bool Convert( string value, out float result )
        {
            if (float.TryParse(value, out var innerResult))
            {
                result = innerResult;
                return true;
            }

            result = default(float);

            return false;
        }
    }
}