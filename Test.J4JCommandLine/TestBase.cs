using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;

namespace J4JSoftware.Binder.Tests
{
    public class TestBase
    {
        private readonly ContainerBuilder _builder = new();

        protected TestBase()
        {
            Container = Configure();

            ParserFactory = Container.Resolve<IParserFactory>();
            LoggerFactory = Container.Resolve<IJ4JLoggerFactory>();
            Logger = LoggerFactory.CreateLogger( GetType() );
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
            builder.RegisterModule( new AutofacModule() );

            builder.RegisterTextToValueAssemblies();
            builder.RegisterTokenAssemblies();
            builder.RegisterMasterTextCollectionAssemblies();
            builder.RegisterBindabilityValidatorAssemblies();
            builder.RegisterCommandLineGeneratorAssemblies();
            builder.RegisterDisplayHelpAssemblies();

            builder.RegisterAssemblyTypes(typeof(IJ4JLogger).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IChannel).IsAssignableFrom(t)
                            && t.GetConstructors().Any(c =>
                            {
                                // constructor must be parameterless
                                var parameters = c.GetParameters();

                                if (parameters.Length != 0)
                                    return false;

                                var attr = t.GetCustomAttribute<ChannelIDAttribute>(false);

                                return attr != null;
                            }))
                .AsImplementedInterfaces()
                .AsSelf()
                .SingleInstance();

            builder.Register(c =>
               {
                   var retVal = new J4JLogger();
                   retVal.Channels.Add(c.Resolve<DebugChannel>());

                   return retVal;
               })
                .As<IJ4JLogger>()
                .AsSelf()
                .SingleInstance();

            //    builder.RegisterType<J4JLoggerFactory>()
            //        .As<IJ4JLoggerFactory>();

            //    builder.RegisterType<ParserFactory>()
            //        .As<IParserFactory>();

            //    RegisterBuiltInTextToValue(builder);

            //    builder.RegisterTypeAssemblies<ITextToValue>(
            //        Enumerable.Empty<Assembly>(),
            //        false,
            //        PredefinedTypeTests.NonAbstract,
            //        PredefinedTypeTests.OnlyJ4JLoggerRequired );

            //    builder.RegisterTypeAssemblies<IAvailableTokens>(
            //        Enumerable.Empty<Assembly>(),
            //        false,
            //        PredefinedTypeTests.NonAbstract,
            //        PredefinedTypeTests.OnlyJ4JLoggerRequired);

            //    builder.RegisterTypeAssemblies<IMasterTextCollection>(
            //        Enumerable.Empty<Assembly>(),
            //        false,
            //        PredefinedTypeTests.NonAbstract,
            //        PredefinedTypeTests.OnlyJ4JLoggerRequired);

            //    builder.RegisterTypeAssemblies<IBindabilityValidator>(
            //        Enumerable.Empty<Assembly>(),
            //        false,
            //        TypeTester.NonAbstract,
            //        new ConstructorTesterPermuted<IBindabilityValidator>(typeof(IJ4JLogger), typeof(IEnumerable<ITextToValue>)));

            //    builder.RegisterTypeAssemblies<IOptionsGenerator>(
            //        Enumerable.Empty<Assembly>(),
            //        false, 
            //        TypeTester.NonAbstract,
            //        new ConstructorTesterPermuted<IOptionsGenerator>(typeof(IJ4JLogger)));

            //    builder.RegisterTypeAssemblies<IDisplayHelp>(
            //        Enumerable.Empty<Assembly>(),
            //        false,
            //        PredefinedTypeTests.NonAbstract,
            //        PredefinedTypeTests.OnlyJ4JLoggerRequired);
            //
        }

        private void RegisterBuiltInTextToValue(ContainerBuilder builder)
        {
            foreach (var convMethod in typeof(Convert)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m =>
                {
                    var parameters = m.GetParameters();

                    return parameters.Length == 1 && !typeof(string).IsAssignableFrom(parameters[0].ParameterType);
                }))
            {
                builder.Register( c =>
                    {
                        var logger = c.IsRegistered<IJ4JLogger>() ? c.Resolve<IJ4JLogger>() : null;

                        var builtInType = typeof( BuiltInTextToValue<> ).MakeGenericType( convMethod.ReturnType );

                        return (ITextToValue) Activator.CreateInstance( builtInType,
                            new object?[] { convMethod, logger } )!;
                    } );
            }
        }

        protected IParserFactory ParserFactory { get; }
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