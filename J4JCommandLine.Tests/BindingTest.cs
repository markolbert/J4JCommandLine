using System.Collections.Generic;
using System.Text;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace J4JCommandLine.Tests
{
    public class BindingTest
    {
        private readonly CommandLineContext _context;

        public BindingTest()
        {
            _context = TestServiceProvider.Instance.GetRequiredService<CommandLineContext>();
        }

        [Fact]
        public void Basic_binding()
        {
            var target = _context.AddBindingTarget( new TestTarget(), "test" );

            target.BindProperty( x => x.IntProperty, -1, "x" );

            _context.Parse( new string[] { "-x", "32" } );
        }
    }
}
