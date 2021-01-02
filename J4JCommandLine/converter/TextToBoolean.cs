namespace J4JSoftware.CommandLine.Deprecated
{
    public class TextToBoolean : TextConverter<bool>
    {
        // Converts a text value to a boolean. Missing text values are assumed
        // to be "true"
        public override bool Convert( string value, out bool result )
        {
            if( string.IsNullOrEmpty( value ) )
            {
                result = true;
                return true;
            }

            if( bool.TryParse( value, out var innerResult ) )
            {
                result = innerResult;
                return true;
            }

            result = false;

            return false;
        }
    }
}