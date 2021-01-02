namespace J4JSoftware.CommandLine.Deprecated
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