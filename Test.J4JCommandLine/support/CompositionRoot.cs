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
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace J4JSoftware.Binder.Tests
{
    public class CompositionRoot : ConsoleRoot
    {
        private static CompositionRoot? _compRoot;

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

        public CompositionRoot()
            : base( "J4JSoftware", "BinderTests",true )
        {
        }

        protected override void ConfigureLogger( J4JLogger logger ) => logger.AddDebug();

        public IJ4JLoggerFactory LoggerFactory => Host!.Services.GetRequiredService<IJ4JLoggerFactory>();
        public J4JLoggerFactory LoggerFactoryNG => Host!.Services.GetRequiredService<J4JLoggerFactory>();
        public IJ4JLogger Logger => Host!.Services.GetRequiredService<IJ4JLogger>();
        public IParserFactory ParserFactory => Host!.Services.GetRequiredService<IParserFactory>();

        public IParser? GetParser( 
            CommandLineStyle style, 
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens ) =>
            ParserFactory.Create( style, out var retVal, textComparison, cleanupTokens ) ? retVal : null;

        public (IConfigurationRoot configRoot, IParser? parser) GetConfigurationAndParser(
            CommandLineStyle style,
            string cmdLine,
            StringComparison? textComparison = null,
            params ICleanupTokens[] cleanupTokens )
        {
            var configRoot = new ConfigurationBuilder()
                .AddJ4JCommandLine(
                    style,
                    cmdLine,
                    Host!.Services,
                    out var parser )
                .Build();

            return ( configRoot, parser );
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.RegisterModule( new AutofacModule() );
            builder.RegisterTextToValueAssemblies();
            builder.RegisterTokenAssemblies();
            builder.RegisterMasterTextCollectionAssemblies();
            builder.RegisterBindabilityValidatorAssemblies();
            builder.RegisterDisplayHelpAssemblies();
        }

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( "appConfig.json" );
        }
    }
}