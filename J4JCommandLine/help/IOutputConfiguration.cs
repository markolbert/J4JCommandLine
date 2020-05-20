namespace J4JSoftware.CommandLine
{
    public interface IOutputConfiguration
    {
        int LineWidth { get; }
        int KeyAreaWidth { get; }
        int DetailAreaWidth { get; }
        int KeyDetailSeparation { get; }
    }
}