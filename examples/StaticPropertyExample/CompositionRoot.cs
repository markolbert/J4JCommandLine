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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace J4JSoftware.CommandLine.Examples
{
    public class CompositionRoot : ConsoleRoot
    {
        private static CompositionRoot? _compRoot;

        private IParserFactory? _parserFactory;
        private IParser? _parser;

        public static CompositionRoot Default
        {
            get
            {
                if( _compRoot != null )
                    return _compRoot;

                _compRoot = new CompositionRoot();
                _compRoot.Build();

                return _compRoot;
            }
        }

        private CompositionRoot()
            : base( "J4JSoftware", "BinderTests",true, osName: OSNames.Linux)
        {
        }

        protected override void ConfigureLogger(J4JLoggerConfiguration loggerConfig)
        {
            loggerConfig.CallingContextToText = ConvertCallingContextToText;
        }

        public IJ4JLogger Logger => Host!.Services.GetRequiredService<IJ4JLogger>();

        public IParserFactory ParserFactory
        {
            get
            {
                _parserFactory ??= Host!.Services.GetRequiredService<IParserFactory>();
                return _parserFactory;
            }
        }

        protected override void ConfigureCommandLineParsing()
        {
            base.ConfigureCommandLineParsing();

            CommandLineOptions!.Bind<Program, int>(x => Program.IntValue, "i")!
                .SetDefaultValue(75)
                .SetDescription("An integer value");

            CommandLineOptions.Bind<Program, string>(x => Program.TextValue, "t")!
                .SetDefaultValue("a cool default")
                .SetDescription("A string value");
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterModule(new AutofacModule());
            builder.RegisterTextToValueAssemblies();
            builder.RegisterTokenAssemblies();
            builder.RegisterMasterTextCollectionAssemblies();
            builder.RegisterBindabilityValidatorAssemblies();
            builder.RegisterCommandLineGeneratorAssemblies();
            builder.RegisterDisplayHelpAssemblies(typeof(HelpDisplayColor));
        }

        // these next two methods serve to strip the project path off of source code
        // file paths
        private static string ConvertCallingContextToText(
            Type? loggedType,
            string callerName,
            int lineNum,
            string srcFilePath)
        {
            return CallingContextEnricher.DefaultConvertToText(loggedType,
                callerName,
                lineNum,
                CallingContextEnricher.RemoveProjectPath(srcFilePath, GetProjectPath()));
        }

        private static string GetProjectPath([CallerFilePath] string filePath = "")
        {
            var dirInfo = new DirectoryInfo(Path.GetDirectoryName(filePath)!);

            while (dirInfo.Parent != null)
            {
                if (dirInfo.EnumerateFiles("*.csproj").Any())
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}