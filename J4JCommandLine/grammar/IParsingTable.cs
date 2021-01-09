namespace J4JSoftware.Configuration.CommandLine
{
    public interface IParsingTable
    {
        bool IsValid { get; }
        TokenEntry.TokenEntries Entries { get; }
        ParsingTable.ParsingAction? this[ TokenType row, TokenType col ] { get; set; }
    }
}