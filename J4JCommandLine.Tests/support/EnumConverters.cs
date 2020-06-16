using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using J4JSoftware.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace J4JCommandLine.Tests
{
    public class PlainEnumConverter : TextToEnum<PlainEnum>
    {
    }

    public class FlagsEnumConverter : TextToEnum<FlagsEnum>
    {
    }
}
