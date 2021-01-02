namespace J4JSoftware.CommandLine.Deprecated
{
    public class TextToLong : TextConverter<long>
    {
        public override bool Convert( string value, out long result )
        {
            if( long.TryParse( value, out var innerResult ) )
            {
                result = innerResult;
                return true;
            }

            result = default;

            return false;
        }
    }
}