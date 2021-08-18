namespace J4JSoftware.Configuration.CommandLine
{
    public record TokenPair
    {
        public TokenPair(
            Token current,
            Token previous
        )
        {
            Current = current;
            Previous = previous;

            TokenTypePair = new TokenTypePair( Current.Type, previous.Type );
        }

        public Token Current { get; }
        public Token Previous { get; }
        public TokenTypePair TokenTypePair { get; }
    }
}