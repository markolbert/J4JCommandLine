namespace J4JSoftware.CommandLine
{
    public class TextToBoolean : TextConverter<bool>
    {
        public override bool Convert( string value, out bool result )
        {
            if( bool.TryParse( value, out var innerResult ) )
            {
                result = innerResult;
                return true;
            }

            result = default;

            return false;
        }
    }
}