using System;

namespace J4JSoftware.CommandLine
{
    public class TextToDateTime : TextConverter<DateTime>
    {
        public override bool Convert( string value, out DateTime result )
        {
            if( DateTime.TryParse( value, out var innerResult ) )
            {
                result = innerResult;
                return true;
            }

            result = default;

            return false;
        }
    }
}