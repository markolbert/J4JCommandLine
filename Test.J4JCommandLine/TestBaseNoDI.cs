using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Configuration.CommandLine.support;
using J4JSoftware.Logging;

namespace J4JSoftware.Binder.Tests
{
    public class TestBaseNoDI
    {
        protected TestBaseNoDI()
        {
            LoggerFactory = new J4JLoggerFactory( () =>
            {
                var retVal = new J4JLogger();
                retVal.AddDebug();

                return retVal;
            } );

            Factory = new J4JCommandLineFactory( loggerFactory: LoggerFactory );

            Logger = LoggerFactory.CreateLogger( GetType() );
        }

        protected J4JCommandLineFactory Factory { get; }
        protected IJ4JLogger Logger { get; }
        protected IJ4JLoggerFactory LoggerFactory { get; }

        protected IOption Bind<TTarget, TProp>(IOptionCollection options, Expression<Func<TTarget, TProp>> propSelector,
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

        protected void CreateOptionsFromContextKeys(IOptionCollection options, IEnumerable<OptionConfig> optConfigs)
        {
            foreach (var optConfig in optConfigs)
            {
                CreateOptionFromContextKey(options, optConfig);
            }
        }

        private void CreateOptionFromContextKey(IOptionCollection options, OptionConfig optConfig)
        {
            var option = options.Add(optConfig.GetPropertyType(), optConfig.ContextPath);
            option.Should().NotBeNull();

            option!.AddCommandLineKey(optConfig.CommandLineKey)
                .SetStyle(optConfig.Style);

            if (optConfig.Required) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;
        }
    }
}