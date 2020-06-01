namespace J4JSoftware.CommandLine
{
    // defines the interface for an object used to parse a command line
    public interface ICommandLineTextParser
    {
        ParseResults Parse( string[] args );
        ParseResults Parse( string cmdLine );
    }
}