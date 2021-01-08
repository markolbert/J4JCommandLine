#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public class EmbeddedTargetNoSetter
    {
        public BasicTargetParameteredCtor Target1 { get; } = new( 0 );
        public BasicTargetParameteredCtor Target2 { get; } = new( 0 );
    }
}