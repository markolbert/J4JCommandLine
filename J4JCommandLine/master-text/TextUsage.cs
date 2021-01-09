namespace J4JSoftware.Configuration.CommandLine
{
    // a class defining how a particular piece of text is uniquely used within
    // the framework
    public class TextUsage
    {
        public TextUsage( string text, TextUsageType usage )
        {
            Text = text;
            Usage = usage;
        }

        public string Text { get; }
        public TextUsageType Usage { get; }
    }
}