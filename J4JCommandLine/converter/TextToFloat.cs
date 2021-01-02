namespace J4JSoftware.CommandLine.Deprecated
{
    public class TextToFloat : TextConverter<float>
    {
        public override bool Convert( string value, out float result )
        {
            if( float.TryParse( value, out var innerResult ) )
            {
                result = innerResult;
                return true;
            }

            result = default;

            return false;
        }
    }
}