using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using J4JSoftware.Configuration.CommandLine;
using J4JSoftware.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace J4JSoftware.Binder.Tests
{
    public class Validator
    {
        protected IOption Bind<TTarget, TProp>( IParser parser, Expression<Func<TTarget, TProp>> propSelector,
            TestConfig testConfig )
            where TTarget : class, new()
        {
            var option = parser.Options.Bind( propSelector );
            option.Should().NotBeNull();

            var optConfig = testConfig.OptionConfigurations
                .FirstOrDefault( x =>
                    option!.ContextPath!.Equals( x.ContextPath, StringComparison.OrdinalIgnoreCase ) );

            optConfig.Should().NotBeNull();

            option!.AddCommandLineKey( optConfig!.CommandLineKey )
                .SetStyle( optConfig.Style );

            if( optConfig.Required ) option.IsRequired();
            else option.IsOptional();

            optConfig.Option = option;

            return option;
        }

        protected void CreateOptionsFromContextKeys( IOptionCollection options, IEnumerable<OptionConfig> optConfigs)
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

        protected void ValidateTokenizing( IParser parser, TestConfig testConfig )
        {
            parser.Parse( testConfig.CommandLine )
                .Should()
                .Be( parser.Options.UnknownKeys.Count == 0 );

            parser.Options.UnknownKeys.Count.Should().Be(testConfig.UnknownKeys);
            parser.Options.UnkeyedValues.Count.Should().Be(testConfig.UnkeyedValues);

            foreach( var optConfig in testConfig.OptionConfigurations )
            {
                optConfig.Option!
                    .ValuesSatisfied
                    .Should()
                    .Be( optConfig.ValuesSatisfied );
            }
        }

        protected void ValidateConfiguration<TParsed>( IConfigurationRoot configRoot, TestConfig testConfig )
            where TParsed : class, new()
        {
            if (testConfig!.OptionConfigurations.Any(x => x.ConversionWillFail))
            {
                var exception = Assert.Throws<InvalidOperationException>(configRoot.Get<TParsed>);
                return;
            }

            var parsed = configRoot.Get<TParsed>();

            if (testConfig.OptionConfigurations.TrueForAll(x => !x.ValuesSatisfied))
            {
                parsed.Should().BeNull();
                return;
            }

            foreach (var optConfig in testConfig.OptionConfigurations)
            {
                GetPropertyValue(parsed, optConfig.ContextPath, out var result, out var resultType)
                    .Should()
                    .BeTrue();

                if (optConfig.Style == OptionStyle.Collection)
                {
                    if (optConfig.CorrectTextArray.Count == 0)
                        result.Should().BeNull();
                    else result.Should().BeEquivalentTo(optConfig.CorrectTextArray);
                }
                else
                {
                    if (optConfig.CorrectText == null)
                    {
                        result.Should().BeNull();
                    }
                    else
                    {
                        var correctValue = resultType!.IsEnum
                            ? Enum.Parse(resultType, optConfig.CorrectText)
                            : Convert.ChangeType(optConfig.CorrectText, resultType!);

                        result.Should().Be(correctValue);
                    }
                }
            }
        }

        private bool GetPropertyValue<TParsed>(
            TParsed parsed,
            string contextKey,
            out object? result,
            out Type? resultType)
            where TParsed : class, new()
        {
            result = null;
            resultType = null;

            Type curType = typeof(TParsed);
            object? curValue = parsed;

            var keys = contextKey.Split(":", StringSplitOptions.RemoveEmptyEntries);

            for (var idx = 0; idx < keys.Length; idx++)
            {
                var curPropInfo = curType.GetProperty(keys[idx]);

                if (curPropInfo == null)
                    return false;

                curType = curPropInfo.PropertyType;
                curValue = curPropInfo.GetValue(curValue);

                if (curValue == null && idx != keys.Length - 1)
                    return false;
            }

            result = curValue;
            resultType = curType;

            return true;
        }
    }
}
