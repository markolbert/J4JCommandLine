namespace J4JSoftware.CommandLine
{
    // defines the interface for an object used to parse a command line
    public interface IAllocator
    {
        AllocationResult AllocateCommandLine( string cmdLine, OptionCollection options );
    }
}