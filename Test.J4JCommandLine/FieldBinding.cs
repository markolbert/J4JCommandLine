using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class FieldBinding : TestBase
    {
        private bool _aSwitch;

        [Fact]
        public void Simple()
        {
            var parser = Parser.GetLinuxDefault( logger: Logger );

            parser.Options.Bind<FieldBinding, bool>( x => x._aSwitch, "x" )
                .Should()
                .NotBeNull();

            parser.Options.FinishConfiguration();

            parser.Parse( "-x" ).Should().BeTrue();

            parser.Options[ "x" ]!.GetValue( out var result ).Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeOfType<bool>();
            ( (bool)result! ).Should().BeTrue();
        }
    }
}
