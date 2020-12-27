namespace J4JSoftware.CommandLine
{
    public enum TokenType
    {
        Separator,
        ValuePrefix,
        KeyPrefix,
        Quoter,

        // Text cannot be created by user code. It only gets
        // created by the tokenizer.
        Text,

        // EndOfInput cannot be created by user code. It only gets
        // created by the tokenizer.
        EndOfInput
    }
}