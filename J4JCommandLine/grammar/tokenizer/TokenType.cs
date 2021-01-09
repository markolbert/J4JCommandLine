namespace J4JSoftware.Configuration.CommandLine
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

        //// EndOfInput cannot be created by user code. It only gets
        //// created by the tokenizer.
        //EndOfInput,

        StartOfInput
        // StartOfInput cannot be created by user code. It only gets
        // created by the tokenizer.
    }
}