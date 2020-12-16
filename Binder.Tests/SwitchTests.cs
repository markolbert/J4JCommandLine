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
    public class SwitchTests : BaseTest
    {
        [Theory]
        [InlineData("ASwitch", "x", "-x", OptionStyle.Switch, true, true, 0, 0)]
        [InlineData("ASwitch", "x", "-z", OptionStyle.Switch, false, false, 1, 0)]
        public void ByContextDefinition( 
            string contextKey, 
            string cmdLineKey, 
            string cmdLine, 
            OptionStyle style, 
            bool allocResult,
            bool valueAssigned,
            int unknownKeys,
            int unkeyedParams )
        {
            var options = CompositionRoot.Default.Options;

            var option = options.Add( new SimpleContextKey( contextKey ) );

            option.AddCommandLineKey( cmdLineKey )
                .SetStyle( style );

            var allocator = CompositionRoot.Default.Allocator;

            var result = allocator.AllocateCommandLine( cmdLine, options );
            
            ( (bool) result ).Should().Be( allocResult );

            option.WasAssignedValue.Should().Be( valueAssigned );

            result.UnknownKeys.Count.Should().Be( unknownKeys );
            result.UnkeyedParameters.Count.Should().Be( unkeyedParams );
        }
    }
}
