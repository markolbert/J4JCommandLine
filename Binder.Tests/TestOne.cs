using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Binder.Tests;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class TestOne : BaseTest
    {
        [Fact]
        public void ATest()
        {
            var options = CompositionRoot.Default.Options;
            var allocator = CompositionRoot.Default.Allocator;
        }
    }
}
