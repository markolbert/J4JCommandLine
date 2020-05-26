namespace J4JSoftware.CommandLine
{
    // configuration information for IHelpErrorProcessor instances.
    public interface IOutputConfiguration
    {
        // the total width of an output line, typically KeyAreaWidth + KeyDetailSeparation + DetailAreaWidth
        int LineWidth { get; }

        // the portion of an output line taken up by the list of command line keys
        int KeyAreaWidth { get; }

        // the portion of an output line taken up by help or error messages
        int DetailAreaWidth { get; }

        // the gap between the key area and the detail area
        int KeyDetailSeparation { get; }
    }
}