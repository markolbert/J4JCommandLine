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
    public class TestBaseNoDI
    {
        protected TestBaseNoDI()
        {
            var loggerConfig = new J4JLoggerConfiguration( FilePathTrimmer );

            loggerConfig.SerilogConfiguration
                .WriteTo.Debug(outputTemplate: loggerConfig.GetOutputTemplate(true));

            Logger = loggerConfig.CreateLogger();
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        protected IOption Bind<TTarget, TProp>(OptionCollection options, Expression<Func<TTarget, TProp>> propSelector,
            TestConfig testConfig)
            where TTarget : class, new()
        {
            var option = options.Bind(propSelector);
            option.Should().NotBeNull();

            var optConfig = testConfig.OptionConfigurations
                .FirstOrDefault(x =>
                    option!.ContextPath!.Equals(x.ContextPath, StringComparison.OrdinalIgnoreCase));

            optConfig.Should().NotBeNull();

            option!.AddCommandLineKey(optConfig!.CommandLineKey)
                .SetStyle(optConfig.Style);

            if (optConfig.Required) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return option;
        }

        //protected void CreateOptionsFromContextKeys(IOptionCollection options, IEnumerable<OptionConfig> optConfigs)
        //{
        //    foreach (var optConfig in optConfigs)
        //    {
        //        CreateOptionFromContextKey(options, optConfig);
        //    }
        //}

        //private void CreateOptionFromContextKey(IOptionCollection options, OptionConfig optConfig)
        //{
        //    var option = options.Add(optConfig.GetPropertyType(), optConfig.ContextPath);
        //    option.Should().NotBeNull();

        //    option!.AddCommandLineKey(optConfig.CommandLineKey)
        //        .SetStyle(optConfig.Style);

        //    if (optConfig.Required) option.IsRequired();
        //    else option.IsOptional();

        //    optConfig.Option = option;
        //}

        // these next two methods serve to strip the project path off of source code
        // file paths
        private static string FilePathTrimmer(
            Type? loggedType,
            string callerName,
            int lineNum,
            string srcFilePath)
        {
            return CallingContextEnricher.DefaultFilePathTrimmer(loggedType,
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