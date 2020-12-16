namespace J4JSoftware.CommandLine
{
    public class SimpleContextKey : IContextKey
    {
        public SimpleContextKey( string contextKey )
        {
            Text = contextKey;
        }

        public string Text { get; }
    }
}