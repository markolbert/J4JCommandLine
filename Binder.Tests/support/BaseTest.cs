using Binder.Tests;
using J4JSoftware.Logging;

namespace J4JSoftware.Binder.Tests
{
    public class BaseTest
    {
        protected BaseTest()
        {
            Logger = CompositionRoot.Default.Logger;
        }

        protected IJ4JLogger Logger { get; }
    }
}
