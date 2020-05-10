namespace J4JSoftware.CommandLine
{
    public interface IBindingTarget
    {
        string Path { get; }
        bool FullyBound { get; set; }
    }
}