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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace J4JSoftware.Binder.Tests;

public static class TestDataSource
{
    public static IEnumerable<object[]> GetSinglePropertyData()
    {
        foreach( var config in GetConfigurations<TestConfig>( "singleProperties.json" ).Skip( 0 ) )
        {
            yield return [config];
        }
    }

    public static IEnumerable<object[]> GetEmbeddedPropertyData()
    {
        foreach( var config in GetConfigurations<TestConfig>( "embeddedProperties.json" ) )
        {
            yield return [config];
        }
    }

    public static IEnumerable<object[]> GetTokenizerData()
    {
        foreach( var config in GetConfigurations<TokenizerConfig>( "tokenizer.json" ) )
        {
            yield return [config];
        }
    }

    public static IEnumerable<object[]> GetParserData()
    {
        foreach( var config in GetConfigurations<TestConfig>( "parser.json" ) )
        {
            yield return [config];
        }
    }

    private static List<T> GetConfigurations<T>( string jsonFile )
        where T : class, new()
    {
        var text = File.ReadAllText( Path.Combine( Environment.CurrentDirectory, "test-files", jsonFile ) );

        return JsonSerializer.Deserialize<List<T>>( text,
                                                    new JsonSerializerOptions
                                                    {
                                                        Converters = { new JsonStringEnumConverter() }
                                                    } )!;
    }
}
