using System;
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
    public enum PlainEnum
    {
        A,
        B,
        C
    }

    [Flags]
    public enum FlagsEnum
    {
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 3,

        None = 0,
        All = A | B | C
    }

    public enum UnconvertibleEnum
    {
        A,
        B,
        C
    }

    public class PlainEnumConverter : TextToEnum<PlainEnum>
    {
    }

    public class FlagsEnumConverter : TextToEnum<FlagsEnum>
    {
    }
}
