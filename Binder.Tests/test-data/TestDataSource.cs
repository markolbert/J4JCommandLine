using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.Binder.Tests
{
    public static class TestDataSource
    {
        public static IEnumerable<object[]> GetSinglePropertyData()
        {
            foreach ( var config in GetConfigurations<TestConfig>("singleProperties.json") )
            {
                yield return new object[] { config };
            }
        }

        public static IEnumerable<object[]> GetEmbeddedPropertyData()
        {
            foreach (var config in GetConfigurations<TestConfig>("embeddedProperties.json"))
            {
                yield return new object[] { config };
            }
        }

        public static IEnumerable<object[]> GetTokenizerData()
        {
            //yield return new object[] { GetConfigurations<TokenizerConfig>( "tokenizer.json" ).Last() };
            foreach (var config in GetConfigurations<TokenizerConfig>("tokenizer.json"))
            {
                yield return new object[] { config };
            }
        }

        private static List<T> GetConfigurations<T>( string jsonFile )
            where T: class, new()
        {
            var text = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, jsonFile));

            return JsonSerializer.Deserialize<List<T>>(
                text,
                new JsonSerializerOptions()
                {
                    Converters = { new JsonStringEnumConverter() }
                })!;
        }
    }
}