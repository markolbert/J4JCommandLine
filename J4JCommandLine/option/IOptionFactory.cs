namespace J4JSoftware.CommandLine
{
    public interface IOptionFactory
    {
        IOption<T>? CreateOption<T>( params string[] keys );
    }
}