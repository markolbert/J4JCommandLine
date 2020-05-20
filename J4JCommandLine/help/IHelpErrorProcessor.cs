namespace J4JSoftware.CommandLine
{
    public interface IHelpErrorProcessor
    {
        void Display( IOptionCollection options, CommandLineErrors errors, string? headerMesg = null );
    }
}