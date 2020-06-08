using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace J4JSoftware.CommandLine
{
    // defines target for binding operations, tying command line arguments to
    // specific properties of TValue
    public class BindingTarget<TValue> : IBindingTarget<TValue>
        where TValue : class
    {
        private readonly ICommandLineParser _parser;
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly MasterTextCollection _masterText;
        private readonly IConsoleOutput _consoleOutput;
        private readonly List<TargetedProperty> _properties = new List<TargetedProperty>();
        private readonly ITargetableTypeFactory _targetableTypeFactory;
        private readonly OptionCollection _options;
        private readonly CommandLineErrors _errors;
        private readonly StringComparison _keyComp;

        private bool _headerDisplayed = false;
        private bool _helpDisplayed = false;
        private bool _outputPending = false;

        // creates an instance tied to the supplied instance of TValue. This allows for binding
        // to more complex objects which may require constructor parameters.
        internal BindingTarget(
            TValue value,
            ICommandLineParser parser,
            IEnumerable<ITextConverter> converters,
            StringComparison keyComp,
            CommandLineErrors errors,
            MasterTextCollection masterText,
            IConsoleOutput consoleOutput
        )
        {
            Value = value;
            _parser = parser;
            _converters = converters;
            _consoleOutput = consoleOutput;
            _keyComp = keyComp;

            _targetableTypeFactory = new TargetableTypeFactory( _converters );

            _masterText = masterText;

            _options = new OptionCollection( _masterText );
            _errors = errors;
        }

        // The instance of TValue being bound to, which was either supplied in the constructor to 
        // this instance or created by it if TValue has a public parameterless constructor
        public TValue Value { get; }

        public string ProgramName { get; internal set; }
        public string Description { get; internal set; }

        public void Initialize()
        {
            _properties.Clear();
            _errors.Clear();
            _options.Clear();
            _headerDisplayed = false;
            _outputPending = false;
            _helpDisplayed = false;
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
        public Option Bind<TProp>(
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
                    _keyComp
                );
            }

            // create an OptionBase object to bind to the "final" property (i.e., the one
            // we're trying to bind to)
            Option? option = null;

            if( property != null )
            {
                _properties.Add(property);

                option = property.TargetableType.Converter == null
                    ? null
                    : new TargetedOption( _options, property.TargetableType );
            }
            // next condition should never be met because there should always be
            // at least one PropertyInfo object and hence one TargetedProperty
            else AddError(keys.First(), $"Final TargetedProperty is undefined");

            // determine whether we were given at least one valid, unique (i.e., so far
            // unused) key
            keys = _options.GetUniqueKeys(keys);

            // if something went wrong create an UntargetedOption to return. These cannot be
            // bound to commandline parameters but serve to capture error information
            if (keys.Length == 0 || option == null || property == null)
                option = new UntargetedOption(_options);

            option.AddKeys(keys);

            _options.Add(option);

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
            var parseResults = _parser.Parse(args);

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

            // safety net
            if ( retVal == MappingResults.Success && _errors.Count > 0 )
                retVal |= MappingResults.UnspecifiedFailure;

            if ( retVal != MappingResults.Success )
            {
                DisplayHeader();
                DisplayErrors();
                DisplayHelp();
            }

            if( parseResults.Any(
                pr => _masterText[TextUsageType.HelpOptionKey].Any(hk=>string.Equals(hk, pr.Key)) ) )
            {
                retVal |= MappingResults.HelpRequested;

                DisplayHeader();
                DisplayHelp();
            }

            if( _outputPending )
                _consoleOutput.Display();

            _errors.Clear();

            return retVal;
        }

        // Utility method for adding errors to the error collection. These are keyed by whatever
        // option key (e.g., the 'x' in '-x') is associated with the error.
        public void AddError( string? key, string error )
        {
            _errors.AddError( this, key, error );
        }

        private void DisplayHeader()
        {
            if( _headerDisplayed )
                return;

            _consoleOutput.Initialize();

            if( !string.IsNullOrEmpty( ProgramName ) )
                _consoleOutput.AddLine( ConsoleSection.Header, ProgramName );

            if( !string.IsNullOrEmpty( Description ) )
                _consoleOutput.AddLine( ConsoleSection.Header, Description );

            _headerDisplayed = true;
            _outputPending = true;
        }

        private void DisplayErrors()
        {
            _consoleOutput.AddLine( ConsoleSection.Errors, "Error(s):" );

            foreach (var error in _errors)
            {
                _consoleOutput.AddError( error.Errors, _masterText.ConjugateKey( error.Source.Key ) );
            }

            _outputPending = true;
        }

        private void DisplayHelp()
        {
            if( _helpDisplayed )
                return;

            var sb = new StringBuilder();

            sb.Append("Command line options");

            switch (_keyComp)
            {
                case StringComparison.Ordinal:
                case StringComparison.InvariantCulture:
                case StringComparison.CurrentCulture:
                    sb.Append(" (case sensitive):");
                    break;

                default:
                    sb.Append(":");
                    break;
            }

            _consoleOutput.AddLine(ConsoleSection.Help, sb.ToString());

            foreach (var option in _options
                .OrderBy(opt => opt.FirstKey)
                .Where(opt => opt.OptionType != OptionType.Null))
            {
                _consoleOutput.AddOption(
                    option.ConjugateKeys(_masterText), 
                    option.Description, 
                    option.DefaultValue?.ToString() );
            }

            _helpDisplayed = true;
            _outputPending = true;
        }

        // allows retrieval of the TValue instance in a type-agnostic way
        object IBindingTarget.GetValue()
        {
            return Value;
        }
    }
}