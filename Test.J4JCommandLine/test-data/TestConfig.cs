﻿#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Test.J4JCommandLine' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;

#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests;

public class TestConfig
{
    public string OperatingSystem { get; set; }
    public string CommandLine { get; set; }
    public int UnknownKeys { get; set; }
    public int UnkeyedValues { get; set; }
    public List<OptionConfig> OptionConfigurations { get; set; }
}
