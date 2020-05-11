namespace J4JSoftware.CommandLine
{
    public interface IBindingTargetFactory
    {
        BindingTarget<T> Create<T>( T? target = null )
            where T : class;
    }
}