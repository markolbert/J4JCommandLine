﻿using J4JSoftware.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    public class BindingTarget<TValue> : IBindingTarget<TValue>
        where TValue : class
    {
        private readonly ICommandLineTextParser _textParser;
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly IHelpErrorProcessor _helpErrorProcessor;
        private readonly IJ4JLogger? _logger;
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly List<TargetedProperty> _properties = new List<TargetedProperty>();
        private readonly ITargetableTypeFactory _targetableTypeFactory;

        public BindingTarget(
            ICommandLineTextParser textParser,
            IEnumerable<ITextConverter> converters,
            IHelpErrorProcessor helpErrorProcessor,
            IParsingConfiguration parseConfig,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            if( !typeof( TValue ).HasPublicParameterlessConstructor() )
                throw new ArgumentException(
                    $"{typeof( TValue )} does not have a public parameterless constructor and cannot be used as a binding target" );

            Value = Activator.CreateInstance<TValue>();
            _textParser = textParser;
            _converters = converters;
            _helpErrorProcessor = helpErrorProcessor;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            ParsingConfiguration = parseConfig;
            _targetableTypeFactory = new TargetableTypeFactory( _converters, loggerFactory?.Invoke() );

            Options = new OptionCollection( ParsingConfiguration, _loggerFactory?.Invoke() );
            Errors = new CommandLineErrors( ParsingConfiguration );
        }

        public BindingTarget(
            TValue value,
            ICommandLineTextParser textParser,
            IEnumerable<ITextConverter> converters,
            IHelpErrorProcessor helpErrorProcessor,
            IParsingConfiguration parseConfig,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            Value = value;
            _textParser = textParser;
            _converters = converters;
            _helpErrorProcessor = helpErrorProcessor;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            ParsingConfiguration = parseConfig;
            _targetableTypeFactory = new TargetableTypeFactory( _converters, loggerFactory?.Invoke() );

            Options = new OptionCollection(ParsingConfiguration, _loggerFactory?.Invoke());
            Errors = new CommandLineErrors(ParsingConfiguration);
        }

        public TValue Value { get; }
        public ReadOnlyCollection<TargetedProperty> TargetedProperties => _properties.ToList().AsReadOnly();
        public IParsingConfiguration ParsingConfiguration { get; }
        public IOptionCollection Options { get; }
        public CommandLineErrors Errors { get; }

        public OptionBase Bind<TProp>(
            Expression<Func<TValue, TProp>> propertySelector,
            params string[] keys )
        {
            // get the PropertyInfo objects defining the path between the binding target's Type
            // and the property we're trying to bind to
            var pathElements = propertySelector.GetPropertyPathInfo();

            TargetedProperty? property = null;

            // walk through the chain of PropertyInfo objects creating TargetedProperty objects
            // for each property. These objects define whether a property is targetable and, if 
            // so, how to bind an Option to it.
            foreach( var pathElement in pathElements )
            {
                property = new TargetedProperty(
                    pathElement,
                    Value,
                    property,
                    _targetableTypeFactory,
                    ParsingConfiguration.TextComparison,
                    _loggerFactory?.Invoke()
                );
            }

            // create an OptionBase object to bind to the "final" property (i.e., the one
            // we're trying to bind to)
            OptionBase? option = null;

            if( property != null )
            {
                _properties.Add(property);

                option = property.TargetableType.Converter == null
                    ? null
                    : new Option(
                        Options,
                        property.TargetableType,
                        _loggerFactory?.Invoke() );

                //option = CreateOption(property.TargetableType);
            }
            // next condition should never be met because there should always be
            // at least one PropertyInfo object and hence one TargetedProperty
            else AddError(keys.First(), $"Final TargetedProperty is undefined");

            // determine whether we were given at least one valid, unique (i.e., so far
            // unused) key
            keys = Options.GetUniqueKeys(keys);

            if (keys.Length == 0)
                _logger?.Error("No unique keys defined for Option");

            // if something went wrong create a NullOption to return. These cannot be
            // bound to commandline parameters but serve to capture error information
            if (keys.Length == 0 || option == null || property == null)
                option = new NullOption(Options, _loggerFactory?.Invoke());

            option.AddKeys(keys);

            Options.Add(option);

            if (property != null)
                property.BoundOption = option;

            return option;
        }

        public MappingResults Parse(string[] args)
        {
            var retVal = MappingResults.Success;

            // parse the arguments into a collection of arguments keyed by the option key
            // note that there can be multiple arguments associated with any option key
            var parseResults = _textParser.Parse(args);

            // scan all the bound options that aren't tied to NullOptions, which are only
            // "bound" in error
            foreach (var property in _properties)
            {
                switch (property.BoundOption!.OptionType)
                {
                    case OptionType.Help:
                        if (property.MapParseResult(this, parseResults, _logger) == MappingResults.Success)
                            retVal |= MappingResults.HelpRequested;
                        break;

                    case OptionType.Mappable:
                        retVal |= property.MapParseResult(this, parseResults, _logger);
                        break;

                    case OptionType.Null:
                        retVal |= MappingResults.Unbound;
                        break;
                }
            }

            if (parseResults.Any(
                pr => ParsingConfiguration.HelpKeys
                    .Any(k => string.Equals(k, pr.Key, ParsingConfiguration.TextComparison))))
                retVal |= MappingResults.HelpRequested;

            _helpErrorProcessor.Display(retVal, this);

            Errors.Clear();

            return retVal;
        }

        public MappingResults MapParseResults( ParseResults parseResults )
        {
            var retVal = MappingResults.Success;

            // scan all the bound options that aren't tied to NullOptions, which are only
            // "bound" in error
            foreach( var property in _properties )
            {
                switch( property.BoundOption!.OptionType )
                {
                    case OptionType.Help:
                        if( property.MapParseResult( this, parseResults, _logger ) == MappingResults.Success )
                            retVal |= MappingResults.HelpRequested;
                        break;

                    case OptionType.Mappable:
                        retVal |= property.MapParseResult(this, parseResults, _logger);
                        break;

                    case OptionType.Null:
                        retVal |= MappingResults.Unbound;
                        break;
                }
            }

            return retVal;
        }

        public void AddError( string key, string error )
        {
            Errors.AddError( this, key, error );
        }

        object IBindingTarget.GetValue()
        {
            return Value;
        }
    }
}