using J4JSoftware.Logging;

namespace J4JSoftware.Configuration.CommandLine;

public class TextToTextConverter : TextToValue<string>
{
    public TextToTextConverter( IJ4JLogger? logger )
        : base( logger )
    {
    }

    protected override bool ConvertTextToValue( string text, out string? result )
    {
        result = text;
        return true;
    }
}
