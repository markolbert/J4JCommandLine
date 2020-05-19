namespace J4JSoftware.CommandLine
{
    public interface ICommandLineTextParser
    {
        ParseResults Parse( string[] args );
    }
}