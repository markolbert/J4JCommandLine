using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace J4JSoftware.CommandLine
{
    // defines target for binding operations, tying command line arguments to
    // specific properties of TValue
    public class BindingTarget<TValue> : IBindingTarget<TValue>
        where TValue : class
    {
        private readonly List<TargetedProperty> _properties = new List<TargetedProperty>();

        private bool _headerDisplayed = false;
        private bool _helpDisplayed = false;
        private bool _outputPending = false;

        // creates an instance tied to the supplied instance of TValue. This allows for binding
        // to more complex objects which may require constructor parameters.
        internal BindingTarget()
        {
        }

        internal ICommandLineParser Parser { get; set; }
        internal IEnumerable<ITextConverter> Converters { get; set; }
        internal ITargetableTypeFactory TypeFactory { get; set; }
        internal OptionCollection Options { get; set; }
        internal CommandLineErrors Errors { get; set; }
        internal StringComparison TextComparison { get; set; }
        internal MasterTextCollection MasterText { get; set; }
        internal IConsoleOutput ConsoleOutput { get; set; }

        public bool IsConfigured => Parser != null && Converters != null && TypeFactory != null && Options != null 
                                    && Errors != null && MasterText != null && ConsoleOutput != null;

        public bool IgnoreUnkeyedParameters { get; internal set; }

        // The instance of TValue being bound to, which was either supplied in the constructor to 
        // this instance or created by it if TValue has a public parameterless constructor
        public TValue Value { get; internal set; }

        public string ProgramName { get; internal set; }
        public string Description { get; internal set; }

        public bool Initialize()
        {
            _properties.Clear();
            
            Errors.Clear();
            Options.Clear();

            _headerDisplayed = false;
            _outputPending = false;
            _helpDisplayed = false;

            return IsConfigured;
        }

        // binds the selected property to a newly-created Option instance. If all goes
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
            if( !IsConfigured )
                return GetUntargetedOption( keys, null, $"{this.GetType().Name} is not configured" );

            // determine whether we were given at least one valid, unique (i.e., so far
            // unused) key
            keys = Options.GetUniqueKeys(keys);

            if( keys.Length == 0 )
                return GetUntargetedOption( keys, null, $"No unique keys defined" );

            var property = GetTargetedProperty(propertySelector.GetPropertyPathInfo());

            if( property.TargetableType.Converter == null )
                return GetUntargetedOption( 
                    keys, 
                    property,
                    $"No converter for {property.TargetableType.SupportedType.Name}" );
            
            var retVal = GetOption( property, true );

            retVal.AddKeys( keys );

            return retVal;
        }

        // binds the selected property to a newly-created Option instance which will enable
        // parsing of all the "non-option" text (i.e., command line parameters not associated with
        // any keyed option) to the selected property.
        //
        // If something goes wrong a NullOption object will be returned. These only serve
        // to capture error information about the binding and parsing efforts.
        //
        // There are a number of reasons why a selected property may not be able to be bound
        // to an Option object. Examples: the property is not publicly read- and write-able; 
        // the property has a null value and does not have a public parameterless constructor
        // to create an instance of it. Check the error output after parsing for details.
        public Option BindUnkeyed<TProp>( Expression<Func<TValue, TProp>> propertySelector )
        {
            if( !IsConfigured )
                return GetUntargetedOption( null, null, $"{this.GetType().Name} is not configured" );

            var property = GetTargetedProperty(propertySelector.GetPropertyPathInfo());

            if( property.TargetableType.Converter == null )
                return GetUntargetedOption( 
                    null, 
                    property,
                    $"No converter for {property.TargetableType.SupportedType.Name}" );

            return GetOption( property, false );
        }

        // Parses the command line arguments against the Option objects bound to 
        // targeted properties, or to NullOption objects to collect error information.
        public MappingResult Parse( string[] args )
        {
            if( !IsConfigured )
            {
                Errors.AddError( this, null, $"{this.GetType().Name} is not configured" );

                DisplayHeader();
                DisplayErrors();
                DisplayHelp();

                return MappingResult.BindingTargetNotConfigured;
            }

            var retVal = MappingResult.Success;

            // parse the arguments into a collection of arguments keyed by the option key
            // note that there can be multiple arguments associated with any option key
            var parseResults = Parser.Parse( args );

            // scan all the bound options that aren't tied to NullOptions, which are only
            // "bound" in error
            foreach( var property in _properties )
            {
                switch ( property.BoundOption!.OptionType )
                {
                    case OptionType.Keyed:
                        // see if our BoundOption's keys match a key in the parse results so we can retrieve a
                        // specific IParseResult
                        var parseResult = parseResults
                            .FirstOrDefault( pr => property.BoundOption
                                .Keys.Any(k => string.Equals(k, pr.Key, TextComparison)) 
                            );

                        retVal |= property.MapParseResult( this, parseResult );

                        break;

                    case OptionType.Unkeyed:
                        // no op, for now; we want to process the single unkeyed option last because that gives
                        // the last keyed option a chance to dump its excess parameters to the unkeyed option
                        break;

                    case OptionType.Null:
                        retVal |= MappingResult.Unbound;
                        break;
                }
            }

            // now process the unkeyed parameters, if any, provided they were bound to a targeted property
            var unkeyed = _properties.FirstOrDefault( p => p.BoundOption?.OptionType == OptionType.Unkeyed );

            if( unkeyed == null )
            {
                if( !IgnoreUnkeyedParameters && parseResults.Unkeyed.NumParameters > 0 )
                {
                    retVal |= MappingResult.UnprocessedUnKeyedParameters;
                    AddError( null, $"{parseResults.Unkeyed.NumParameters:n0} unprocessed parameter(s)" );
                }
            }
            else retVal |= unkeyed.MapParseResult( this, parseResults.Unkeyed );

            // safety net
            if ( retVal == MappingResult.Success && Errors.Count > 0 )
                retVal |= MappingResult.UnspecifiedFailure;

            if( retVal != MappingResult.Success )
            {
                DisplayHeader();
                DisplayErrors();
                DisplayHelp();
            }

            if( parseResults.Any(
                pr => MasterText[ TextUsageType.HelpOptionKey ].Any( hk => string.Equals( hk, pr.Key ) ) ) )
            {
                retVal |= MappingResult.HelpRequested;

                DisplayHeader();
                DisplayHelp();
            }

            if( _outputPending )
                ConsoleOutput.Display();

            Errors.Clear();

            return retVal;
        }

        // Utility method for adding errors to the error collection. These are keyed by whatever
        // option key (e.g., the 'x' in '-x') is associated with the error.
        public void AddError( string? key, string error )
        {
            Errors.AddError( this, key, error );
        }

        private TargetedProperty GetTargetedProperty( List<PropertyInfo> pathElements )
        {
            TargetedProperty? retVal = null;

            // walk through the chain of PropertyInfo objects creating TargetedProperty objects
            // for each property. These objects define whether a property is targetable and, if 
            // so, how to bind an Option to it.
            foreach (var pathElement in pathElements)
            {
                retVal = new TargetedProperty(
                    pathElement,
                    Value,
                    retVal,
                    TypeFactory,
                    TextComparison
                );
            }

            if (retVal == null)
                throw new NullReferenceException($"Could not create final TargetedProperty");

            _properties.Add(retVal);

            return retVal;
        }

        private Option GetOption( TargetedProperty property, bool isKeyed )
        {
            var style = OptionStyle.SingleValued;

            if( property.TargetableType.IsCollection )
                style = OptionStyle.Collection;
            else
            {
                if( property.PropertyInfo.PropertyType == typeof(bool) )
                    style = OptionStyle.Switch;
            }

            var retVal = new MappableOption( Options, property.TargetableType, isKeyed )
            {
                OptionStyle = style
            };

            // create an Option object to bind to the "final" property (i.e., the one
            // we're trying to bind to)

            Options.Add( retVal );

            property.BoundOption = retVal;

            return retVal;
        }

        private Option GetUntargetedOption( string[]? keys, TargetedProperty? property, string error )
        {
            var retVal = new UntargetedOption( Options );

            if( keys != null )
                retVal.AddKeys( keys );

            Options.Add( retVal );

            if( property != null )
                property.BoundOption = retVal;

            Errors.AddError( this, keys.FirstOrDefault(), error );

            return retVal;
        }

        private void DisplayHeader()
        {
            if( _headerDisplayed )
                return;

            ConsoleOutput.Initialize();

            if( !string.IsNullOrEmpty( ProgramName ) )
                ConsoleOutput.AddLine( ConsoleSection.Header, ProgramName );

            if( !string.IsNullOrEmpty( Description ) )
                ConsoleOutput.AddLine( ConsoleSection.Header, Description );

            _headerDisplayed = true;
            _outputPending = true;
        }

        private void DisplayErrors()
        {
            ConsoleOutput.AddLine( ConsoleSection.Errors, "Error(s):" );

            foreach( var error in Errors )
            {
                ConsoleOutput.AddError( error.Errors, MasterText.ConjugateKey( error.Source.Key ) );
            }

            _outputPending = true;
        }

        private void DisplayHelp()
        {
            if( _helpDisplayed )
                return;

            var sb = new StringBuilder();

            sb.Append( "Command line options" );

            switch( TextComparison )
            {
                case StringComparison.Ordinal:
                case StringComparison.InvariantCulture:
                case StringComparison.CurrentCulture:
                    sb.Append( " (case sensitive):" );
                    break;

                default:
                    sb.Append( ":" );
                    break;
            }

            ConsoleOutput.AddLine( ConsoleSection.Help, sb.ToString() );

            foreach( var option in Options
                .OrderBy( opt => opt.FirstKey )
                .Where( opt => opt.OptionType != OptionType.Null ) )
            {
                ConsoleOutput.AddOption(
                    option.ConjugateKeys( MasterText ),
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