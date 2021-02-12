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
using J4JSoftware.DependencyInjection;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.Binder.Tests
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        static CompositionRoot()
        {
            Default = new CompositionRoot();

            Default.Initialize();
        }

        private CompositionRoot()
            : base( "J4JSoftware", "BinderTests" )
        {
            UseConsoleLifetime = true;

            var loggerConfig = new J4JLoggerConfiguration
            {
                SourceRootPath = "C:/Programming/J4JCommandLine"
            };

            loggerConfig.Channels.Add( new DebugConfig() );

            StaticConfiguredLogging( loggerConfig );
        }

        public static CompositionRoot Default { get; }

        public Func<IJ4JLogger> LoggerFactory => () => Host!.Services.GetRequiredService<IJ4JLogger>();

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.SetBasePath( Environment.CurrentDirectory )
                .AddJsonFile( "appConfig.json" );
        }
    }
}