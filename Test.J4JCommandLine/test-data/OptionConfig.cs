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
        ListOfStrings
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

        public Type GetPropertyType() => PropertyType switch
        {
            PropertyTypes.Boolean => typeof(bool),
            PropertyTypes.Int => typeof(int),
            PropertyTypes.ListOfStrings => typeof(List<string>),
            PropertyTypes.TestEnum => typeof(TestEnum),
            PropertyTypes.TestFlagEnum => typeof(TestFlagEnum),
            _ => typeof(string)
        };

        public IOption? Option { get; set; }
    }
}