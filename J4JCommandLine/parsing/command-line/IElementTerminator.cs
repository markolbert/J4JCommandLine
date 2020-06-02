namespace J4JSoftware.CommandLine
{
    public interface IElementTerminator
    {
        int GetMaxTerminatorLength( string text, bool isKey );
    }
}