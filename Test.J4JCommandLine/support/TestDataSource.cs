using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.Binder.Tests
{
    public static class TestDataSource
    {
        public static IEnumerable<object[]> GetSinglePropertyData()
        {
            foreach( var config in GetConfigurations<TestConfig>( "singleProperties.json" ).Skip( 0 ) )
                yield return new object[] { config };
        }

        public static IEnumerable<object[]> GetEmbeddedPropertyData()
        {
            foreach( var config in GetConfigurations<TestConfig>( "embeddedProperties.json" ) )
                yield return new object[] { config };
        }

        public static IEnumerable<object[]> GetTokenizerData()
        {
            foreach( var config in GetConfigurations<TokenizerConfig>( "tokenizer.json" ) )
                yield return new object[] { config };
        }

        public static IEnumerable<object[]> GetParserData()
        {
            foreach( var config in GetConfigurations<TestConfig>( "parser.json" ) )
                yield return new object[] { config };
        }

        private static List<T> GetConfigurations<T>( string jsonFile )
            where T : class, new()
        {
            var text = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, "test-files", jsonFile ) );

            return JsonSerializer.Deserialize<List<T>>(
                text,
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                } )!;
        }
    }
}