using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binder.Tests;
using J4JSoftware.Logging;
using Microsoft.Extensions.Hosting;

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
