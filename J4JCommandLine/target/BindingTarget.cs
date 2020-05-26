using J4JSoftware.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace J4JSoftware.CommandLine
{
    // defines target for binding operations, tying command line arguments to
    // specific properties of TValue
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

        // attempts to create an instance tied to a dynamically-created instance
        // of TValue. If TValue lacks a parameterless public constructor an ArgumentException
        // is thrown.
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

        // creates an instance tied to the supplied instance of TValue. This allows for binding
        // to more complex objects which may require constructor parameters.
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

        // The instance of TValue being bound to, which was either supplied in the constructor to 
        // this instance or created by it if TValue has a public parameterless constructor
        public TValue Value { get; }

        // the properties targeted by this binding operation (i.e., ones tied to particular OptionBase objects)
        public ReadOnlyCollection<TargetedProperty> TargetedProperties => _properties.ToList().AsReadOnly();

        public IParsingConfiguration ParsingConfiguration { get; }

        // the IOption objects created by binding properties to TValue
        public IOptionCollection Options { get; }

        // Errors encountered during the binding or parsing operations
        public CommandLineErrors Errors { get; }

        // binds the selected property to a newly-created OptionBase instance. If all goes
        // well that will be an Option object capable of being a valid parsing target. If
        // something goes wrong a NullOption object will be returned. These only serve
        // to capture error information about the binding and parsing efforts.
        //
        // There are a number of reasons why a selected property may not be able to be bound
        // to an Option object. Examples: the property is not publicly read- and write-able; 
        // the property has a null value and does not have a public parameterless constructor
        // to create an instance of it. Check the error output after parsing for details.
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

        // Parses the command line arguments against the Option objects bound to 
        // targeted properties, or to NullOption objects to collect error information.
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

        // Utility method for adding errors to the error collection. These are keyed by whatever
        // option key (e.g., the 'x' in '-x') is associated with the error.
        public void AddError( string key, string error )
        {
            Errors.AddError( this, key, error );
        }

        // allows retrieval of the TValue instance in a type-agnostic way
        object IBindingTarget.GetValue()
        {
            return Value;
        }
    }
}