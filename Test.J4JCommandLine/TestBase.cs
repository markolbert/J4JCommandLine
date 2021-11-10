using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Serilog;

namespace J4JSoftware.Binder.Tests
{
    public class TestBase
    {
        private readonly ContainerBuilder _builder = new();

        protected TestBase()
        {
            Container = Configure();

            Logger = Container.Resolve<IJ4JLogger>();
            Logger.SetLoggedType( GetType() );
        }

        protected IContainer Container { get; }
        protected IJ4JLogger Logger { get; }

        private IContainer Configure()
        {
            ConfigureContainer( _builder );

            return _builder.Build();
        }

        protected virtual void ConfigureContainer( ContainerBuilder builder )
        {
            builder.Register( c =>
                              {
                                  var loggerConfig = new J4JLoggerConfiguration( FilePathTrimmer );

                                  loggerConfig.SerilogConfiguration
                                              .WriteTo.Debug( outputTemplate: loggerConfig.GetOutputTemplate( true ) );

                                  return loggerConfig.CreateLogger();
                              } )
                   .AsImplementedInterfaces()
                   .SingleInstance();
        }

        private void RegisterBuiltInTextToValue( ContainerBuilder builder )
        {
            foreach ( var convMethod in typeof( Convert )
                                        .GetMethods( BindingFlags.Static | BindingFlags.Public )
                                        .Where( m =>
                                                {
                                                    var parameters = m.GetParameters();

                                                    return parameters.Length == 1
                                                           && !typeof( string ).IsAssignableFrom( parameters[ 0 ]
                                                               .ParameterType );
                                                } ) )
            {
                builder.Register( c =>
                                  {
                                      var logger = c.IsRegistered<IJ4JLogger>() ? c.Resolve<IJ4JLogger>() : null;

                                      var builtInType =
                                          typeof( BuiltInTextToValue<> ).MakeGenericType( convMethod.ReturnType );

                                      return (ITextToValue) Activator.CreateInstance( builtInType,
                                       new object?[] { convMethod, logger } )!;
                                  } );
            }
        }

        protected IOption Bind<TTarget, TProp>( OptionCollection options,
                                                Expression<Func<TTarget, TProp>> propSelector,
                                                TestConfig testConfig )
            where TTarget : class, new()
        {
            var option = options.Bind( propSelector );
            option.Should().NotBeNull();

            var optConfig = testConfig.OptionConfigurations
                                      .FirstOrDefault( x =>
                                                           option!.ContextPath!.Equals( x.ContextPath,
                                                            StringComparison.OrdinalIgnoreCase ) );

            optConfig.Should().NotBeNull();

            option!.AddCommandLineKey( optConfig!.CommandLineKey )
                   .SetStyle( optConfig.Style );

            if ( optConfig.Required ) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return option;
        }

        // these next two methods serve to strip the project path off of source code
        // file paths
        private static string FilePathTrimmer( Type? loggedType,
                                               string callerName,
                                               int lineNum,
                                               string srcFilePath )
        {
            return CallingContextEnricher.DefaultFilePathTrimmer( loggedType,
                                                                 callerName,
                                                                 lineNum,
                                                                 CallingContextEnricher.RemoveProjectPath( srcFilePath,
                                                                  GetProjectPath() ) );
        }

        private static string GetProjectPath( [ CallerFilePath ] string filePath = "" )
        {
            var dirInfo = new DirectoryInfo( Path.GetDirectoryName( filePath )! );

            while ( dirInfo.Parent != null )
            {
                if ( dirInfo.EnumerateFiles( "*.csproj" ).Any() )
                    break;

                dirInfo = dirInfo.Parent;
            }

            return dirInfo.FullName;
        }
    }
}
