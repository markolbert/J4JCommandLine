using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class BindingTarget<TValue> : IBindingTarget<TValue>
        where TValue : class
    {
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly IParsingConfiguration _parseConfig;
        private readonly CommandLineErrors _errors;
        private readonly StringComparison _keyComp;
        private readonly IJ4JLogger? _logger;
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly IOptionCollection _options;
        private readonly TargetableProperties _properties = new TargetableProperties();

        public BindingTarget(
            string targetID,
            TValue value,
            IEnumerable<ITextConverter> converters,
            IOptionCollection options,
            IParsingConfiguration parseConfig,
            CommandLineErrors errors,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            ID = targetID;
            Value = value;
            _converters = converters;
            _options = options;
            _parseConfig = parseConfig;
            _errors = errors;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            _keyComp = parseConfig.TextComparison;

            Initialize();
        }

        public BindingTarget(
            string targetID,
            IEnumerable<ITextConverter> converters,
            IOptionCollection options,
            IParsingConfiguration parseConfig,
            CommandLineErrors errors,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            ID = targetID;
            _converters = converters;
            _options = options;
            _parseConfig = parseConfig;
            _errors = errors;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            // if TTarget can't be created we have to abort
            if( !typeof(TValue).HasPublicParameterlessConstructor() )
                throw new ApplicationException( $"Couldn't create and instance of {typeof(TValue)}" );

            Value = Activator.CreateInstance<TValue>();

            _keyComp = parseConfig.TextComparison;

            Initialize();
        }

        public TValue Value { get; }
        public string ID { get; }
        public ReadOnlyCollection<TargetableProperty> TargetableProperties => _properties.ToList().AsReadOnly();

        public OptionBase BindProperty<TProp>(
            Expression<Func<TValue, TProp>> propertySelector,
            object? defaultValue,
            params string[] keys )
        {
            var (propPath, propType) = propertySelector.GetPropertyPathAndType();

            return typeof(ICollection).IsAssignableFrom( propType )
                ? BindPropertyCollection( propPath!, keys )
                : BindProperty( propPath!, defaultValue!, keys );
        }

        public OptionBase BindProperty(
            string propertyPath,
            object? defaultValue,
            params string[] keys ) => CreateSingleOption( propertyPath, keys, defaultValue );

        public OptionBase BindPropertyCollection(
            string propertyPath,
            params string[] keys ) => CreateCollectionOption( propertyPath, keys );

        public MappingResults MapParseResults( ParseResults parseResults )
        {
            var retVal = MappingResults.Success;

            // scan all the bound options that aren't tied to NullOptions, which are only
            // "bound" in error
            foreach( var boundProp in _properties.Where( p => p.BoundOption != null ) )
            {
                switch( boundProp.BoundOption!.OptionType )
                {
                    case OptionType.Help:
                        if( boundProp.MapParseResult( this, parseResults, _logger ) == MappingResults.Success )
                            retVal |= MappingResults.HelpRequested;
                        break;

                    case OptionType.Mappable:
                        retVal |= boundProp.MapParseResult(this, parseResults, _logger);
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
            _errors.AddError( this, key, error );
        }

        object IBindingTarget.GetValue()
        {
            return Value;
        }

        private void Initialize()
        {
            var type = typeof(TValue);

            _logger?.Verbose( "Finding targetable properties for {type}", type );

            ScanProperties( type, Value, new Stack<PropertyInfo>() );
        }

        private void ScanProperties<TScan>( Type containerType, TScan container, Stack<PropertyInfo> pathToContainer )
        {
            foreach( var property in containerType.GetProperties() )
            {
                var curTP = TargetableProperty.Create( property, container, _keyComp, pathToContainer, _logger );

                if( !curTP.IsTargetable )
                {
                    _logger?.Verbose<string>( "Property {0} is not targetable", property.Name );
                    continue;
                }

                _properties.Add( curTP );

                // if the property isn't defined in the container, create it so we can 
                // traverse any properties it may have
                object? child = null;

                if( curTP.IsDefined )
                {
                    child = property.GetValue( container );
                }
                else
                {
                    // we only need to create properties that are SingleValues because
                    // those are the only ones we recurse into looking for targetable properties
                    if( curTP.Multiplicity == PropertyMultiplicity.SingleValue )
                    {
                        child = Activator.CreateInstance( property.PropertyType );
                        property.SetValue( container, child );
                    }
                }

                _logger?.Information<string>( "Found targetable property {0}", property.Name );

                // recurse over any child properties of the current property provided it's a
                // SingleValue property but not a ValueType (which don't have child properties)
                if( curTP.Multiplicity == PropertyMultiplicity.SingleValue
                    && !typeof(ValueType).IsAssignableFrom( property.PropertyType ) )
                {
                    _logger?.Verbose<string>( "Finding targetable properties and methods for {0}", property.Name );

                    pathToContainer.Push( property );

                    ScanProperties( property.PropertyType, child, pathToContainer );
                }
            }

            if( pathToContainer.Count > 0 )
                pathToContainer.Pop();
        }

        // Creates an Option if a TargetableProperty based on a supported single value exists with the specified propertyPath
        private OptionBase CreateSingleOption( string propertyPath, string[] keys, object? defaultValue )
        {
            OptionBase? retVal = null;

            // see if the property we want to bind is targetable
            var property = _properties.GetProperty(propertyPath);

            if( property == null )
            {
                _logger?.Error<string>(
                    "Attempted to bind to complex property '{0}', which is not supported", propertyPath );

                property = new TargetableProperty(_keyComp, _properties.Count + 1 );
                _properties.Add( property );

                var key = keys.FirstOrDefault() ?? "?";
                AddError(key, $"Attempted to bind to complex property '{propertyPath}', which is not supported");

                return FinalizeAndStoreOption(property, retVal, keys);
            }

            // check that it's the right multiplicity
            if (!property.Multiplicity.IsTargetableSingleValue())
                _logger?.Error<string>( "Property '{propertyPath}' is not single-value", propertyPath );
            else
            {
                retVal = CreateOption( property.PropertyInfo.PropertyType );

                if( !( retVal is NullOption ) && defaultValue != null )
                    retVal.SetDefaultValue( defaultValue );
            }

            return FinalizeAndStoreOption( property, retVal, keys );
        }

        // Creates an Option if a TargetableProperty based on a supported collection exists with the specified propertyPath
        private OptionBase CreateCollectionOption( string propertyPath, string[] keys )
        {
            OptionBase? retVal = null;

            // see if the property we want to bind is targetable
            var property = _properties.GetProperty( propertyPath );

            if( property == null )
            {
                _logger?.Error<string>(
                    "Attempted to bind to complex property '{0}', which is not supported", propertyPath);

                property = new TargetableProperty(_keyComp, _properties.Count + 1);
                _properties.Add(property);

                var key = keys.FirstOrDefault() ?? "?";
                AddError(key, $"Attempted to bind to complex property '{propertyPath}', which is not supported");

                return FinalizeAndStoreOption(property, retVal, keys);
            }

            // check that it's the right multiplicity
            if( !property.Multiplicity.IsTargetableCollection() )
                _logger?.Error<string>( "Property '{propertyPath}' is not a collection", propertyPath );
            else
            {
                // we need to find the Type on which the collection is based
                var propType = property.Multiplicity == PropertyMultiplicity.Array
                    ? property.PropertyInfo.PropertyType.GetElementType()
                    : property.PropertyInfo.PropertyType.GenericTypeArguments.First();

                retVal = CreateOption( propType! );
            }

            return FinalizeAndStoreOption( property, retVal, keys );
        }

        // creates an option for the specified Type provided an ITextConverter for
        // that Type exists. Otherwise, returns a NullOption
        private OptionBase CreateOption(Type propType)
        {
            OptionBase? retVal = null;

            var converter = _converters.FirstOrDefault(c => c.SupportedType == propType);

            if (converter == null)
                _logger?.Error("No ITextConverter exists for Type {0}", propType);
            else
                retVal = new Option(_options, converter, _loggerFactory?.Invoke());

            return retVal ?? new NullOption(_options, _loggerFactory?.Invoke());
        }

        // ensures option exists and has at least one valid, unique key.
        // returns a NullOption if not. Stores the new option in the options
        // collection.
        private OptionBase FinalizeAndStoreOption( TargetableProperty property, OptionBase? option, string[] keys )
        {
            keys = _options.GetUniqueKeys( keys );

            if( keys.Length == 0 )
                _logger?.Error( "No unique keys defined for Option" );

            if( keys.Length == 0 || option == null )
                option = new NullOption( _options, _loggerFactory?.Invoke() );

            option.AddKeys( keys );

            _options.Add( option );

            property.BoundOption = option;

            return option;
        }
    }
}