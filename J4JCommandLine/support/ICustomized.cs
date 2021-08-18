namespace J4JSoftware.Configuration.CommandLine
{
    public interface ICustomized
    {
        Customization Customization { get; }
        int Priority { get; }
    }
}