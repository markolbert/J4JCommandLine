using System;
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
        private readonly List<TargetedProperty> _properties = new List<TargetedProperty>();
        private readonly ITargetableTypeFactory _targetableTypeFactory;

        // attempts to create an instance tied to a dynamically-created instance
        // of TValue. If TValue lacks a parameterless public constructor an ArgumentException
        // is thrown.
        internal BindingTarget(
            ICommandLineTextParser textParser,
            IEnumerable<ITextConverter> converters,
            IHelpErrorProcessor helpErrorProcessor,
            StringComparison keyComp,
            CommandLineErrors errors
        )
        {
            if( !typeof( TValue ).HasPublicParameterlessConstructor() )
                throw new ArgumentException(
                    $"{typeof( TValue )} does not have a public parameterless constructor and cannot be used as a binding target" );

            Value = Activator.CreateInstance<TValue>();
            _textParser = textParser;
            _converters = converters;
            _helpErrorProcessor = helpErrorProcessor;
            KeyComparison = keyComp;

            _targetableTypeFactory = new TargetableTypeFactory( _converters );

            Options = new OptionCollection( KeyComparison );
            Errors = errors;
        }

        // creates an instance tied to the supplied instance of TValue. This allows for binding
        // to more complex objects which may require constructor parameters.
        internal BindingTarget(
            TValue value,
            ICommandLineTextParser textParser,
            IEnumerable<ITextConverter> converters,
            IHelpErrorProcessor helpErrorProcessor,
            StringComparison keyComp,
            CommandLineErrors errors
        )
        {
            Value = value;
            _textParser = textParser;
            _converters = converters;
            _helpErrorProcessor = helpErrorProcessor;
            KeyComparison = keyComp;

            _targetableTypeFactory = new TargetableTypeFactory( _converters );

            Options = new OptionCollection( KeyComparison );
            Errors = errors;
        }

        // The instance of TValue being bound to, which was either supplied in the constructor to 
        // this instance or created by it if TValue has a public parameterless constructor
        public TValue Value { get; }

        public StringComparison KeyComparison { get; }

        public string ProgramName { get; internal set; }
        public string Description { get; internal set; }

        // the properties targeted by this binding operation (i.e., ones tied to particular OptionBase objects)
        public ReadOnlyCollection<TargetedProperty> TargetedProperties => _properties.ToList().AsReadOnly();

        // the IOption objects created by binding properties to TValue
        public IOptionCollection Options { get; }

        // Errors encountered during the binding or parsing operations
        public CommandLineErrors Errors { get; }

        public void Initialize()
        {
            _properties.Clear();
            Errors.Clear();
            Options.Clear();
        }

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
                    KeyComparison
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
                    : new Option( Options, property.TargetableType );
            }
            // next condition should never be met because there should always be
            // at least one PropertyInfo object and hence one TargetedProperty
            else AddError(keys.First(), $"Final TargetedProperty is undefined");

            // determine whether we were given at least one valid, unique (i.e., so far
            // unused) key
            keys = Options.GetUniqueKeys(keys);

            // if something went wrong create a NullOption to return. These cannot be
            // bound to commandline parameters but serve to capture error information
            if (keys.Length == 0 || option == null || property == null)
                option = new NullOption(Options);

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
                        retVal |= property.MapParseResult(this, parseResults);
                        break;

                    case OptionType.Null:
                        retVal |= MappingResults.Unbound;
                        break;
                }
            }

            if( parseResults.Any(
                pr => _helpErrorProcessor.HelpKeys.HasText( pr.Key ) ) )
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