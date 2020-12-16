using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Binder.Tests;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class SingleValueTests : BaseTest
    {
        [Theory]
        [InlineData("ASingleValue", "x", "-x", false, 0, 0)]
        [InlineData("ASingleValue", "x", "-z", false, 1, 0)]
        [InlineData("ASingleValue", "x", "-x expected", true, 0, 0)]
        [InlineData("ASingleValue", "x", "-z expected", false, 1, 1)]
        [InlineData("ASingleValue", "x", "-x expected excess", true, 0, 1)]
        [InlineData("ASingleValue", "x", "-z expected excess", false, 1, 2)]
        public void ByContextDefinition( 
            string contextKey, 
            string cmdLineKey, 
            string cmdLine, 
            bool valuesSatisfied,
            int unknownKeys,
            int unkeyedParams )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( contextKey );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( OptionStyle.SingleValued );

            var allocator = CompositionRoot.Default.Allocator;

            var result = allocator.AllocateCommandLine( cmdLine, options );
            
            option.ValuesSatisfied.Should().Be( valuesSatisfied );

            result.UnknownKeys.Count.Should().Be( unknownKeys );
            result.UnkeyedParameters.Count.Should().Be( unkeyedParams );
        }
    }
}
