#region license

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

using System;
using System.Collections.Generic;
using J4JSoftware.Configuration.CommandLine;

#pragma warning disable 8618

namespace J4JSoftware.Binder.Tests
{
    public enum PropertyTypes
    {
        Boolean,
        String,
        Int,
        TestEnum,
        TestFlagEnum,
        ListOfStrings,
        ListOfEnums
    }

    public class OptionConfig
    {
        public string CommandLineKey { get; set; }
        public string ContextPath { get; set; }
        public OptionStyle Style { get; set; }
        public bool Required { get; set; }
        public bool ConversionWillFail { get; set; }
        public bool ValuesSatisfied { get; set; }
        public string? CorrectText { get; set; }
        public List<string> CorrectTextArray { get; set; }
        public PropertyTypes PropertyType { get; set; }

        public IOption? Option { get; set; }

        public Type GetPropertyType()
        {
            return PropertyType switch
            {
                PropertyTypes.Boolean => typeof(bool),
                PropertyTypes.Int => typeof(int),
                PropertyTypes.ListOfStrings => typeof(List<string>),
                PropertyTypes.TestEnum => typeof(TestEnum),
                PropertyTypes.TestFlagEnum => typeof(TestFlagEnum),
                PropertyTypes.ListOfEnums => typeof(List<TestEnum>),
                _ => typeof(string)
            };
        }
    }
}