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
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class ConfigurationParserTests : Validator
    {
        private IParser _parser;
        private IConfigurationRoot _configRoot;
        private CommandLineSource _cmdLineSource = new();

        public ConfigurationParserTests(
            IServiceProvider svcProvider,
            IParserFactory parserFactory,
            IJ4JLoggerFactory loggerFactory
        )
        {
            _configRoot = new ConfigurationBuilder()
                .AddJ4JCommandLine(
                    CommandLineStyle.Linux,
                    _cmdLineSource,
                    svcProvider,
                    out var temp)
                .Build();

            temp.Should().NotBeNull();
            _parser = temp!;
        }

        [ Theory ]
        [ MemberData( nameof(TestDataSource.GetParserData), MemberType = typeof(TestDataSource) ) ]
        public void ParserTest( TestConfig testConfig )
        {
            CreateOptionsFromContextKeys(_parser!.Options, testConfig.OptionConfigurations);
            _parser.Options.FinishConfiguration();
            _parser.Options.Count.Should().BeGreaterThan(0);

            _parser.Parse( testConfig.CommandLine ).Should().BeTrue();

            ValidateConfiguration<BasicTarget>( _configRoot, testConfig );
        }
    }
}