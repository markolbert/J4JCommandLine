using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using J4JSoftware.CommandLine;

namespace J4JSoftware.Binder.Tests
{
    public static class TestDataSource
    {
        public static IEnumerable<object[]> GetSinglePropertyData()
        {
            foreach ( var config in GetConfigurations("singleProperties.json") )
            {
                yield return new object[] { config };
            }
        }

        private static List<TestConfig> GetConfigurations( string jsonFile )
        {
            var text = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, jsonFile));

            return JsonSerializer.Deserialize<List<TestConfig>>(
                text,
                new JsonSerializerOptions()
                {
                    Converters = { new JsonStringEnumConverter() }
                })!;
        }
    }
}