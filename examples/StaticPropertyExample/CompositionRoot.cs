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
using Autofac;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.J4JCommandLine;
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.CommandLine.Examples
{
    public class CompositionRoot : ConsoleRoot
    {
        private static CompositionRoot? _compRoot;

        private IParserFactory? _parserFactory;
        private IParser? _parser;
        private IDisplayHelp? _displayHelp;

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
            : base( "J4JSoftware", "BinderTests",true )
        {
        }

        protected override void ConfigureLogger( J4JLogger logger )
        {
            logger.AddDebug();
            logger.AddConsole();
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

        public IParser Parser
        {
            get
            {
                if( _parser == null )
                {
                    if( !ParserFactory.Create( OSNames.Windows, out var temp ) )
                        throw new InvalidOperationException( $"Could not create an instance of IParser" );

                    _parser = temp;
                }

                return _parser!;
            }
        }

        public IDisplayHelp DisplayHelp
        {
            get
            {
                _displayHelp = new DisplayColorHelp( Logger );
                return _displayHelp;
            }
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterModule( new AutofacModule() );
            builder.RegisterTextToValueAssemblies();
            builder.RegisterTokenAssemblies();
            builder.RegisterMasterTextCollectionAssemblies();
            builder.RegisterBindabilityValidatorAssemblies();
        }

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( "appConfig.json" );
        }
    }
}