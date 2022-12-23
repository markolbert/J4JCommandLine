namespace J4JSoftware.Configuration.CommandLine;

public record TokenPair
{
    public TokenPair( Token current,
        Token previous )
    {
        Current = current;
        Previous = previous;

        LexicalPair = new LexicalPair( Current.Type, previous.Type );
    }

    public Token Current { get; }
    public Token Previous { get; }
    public LexicalPair LexicalPair { get; }
}