using J4JSoftware.Logging;
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
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly CommandLineErrors _errors;
        private readonly StringComparison _keyComp;
        private readonly IJ4JLogger? _logger;
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly IOptionCollection _options;
        private readonly List<TargetedProperty> _properties = new List<TargetedProperty>();
        private readonly ITargetableTypeFactory _targetableTypeFactory;

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
            _errors = errors;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            _keyComp = parseConfig.TextComparison;
            _targetableTypeFactory = new TargetableTypeFactory( _converters, loggerFactory?.Invoke() );
            //Initialize();
        }

        //public BindingTarget(
        //    string targetID,
        //    IEnumerable<ITextConverter> converters,
        //    IOptionCollection options,
        //    IParsingConfiguration parseConfig,
        //    CommandLineErrors errors,
        //    Func<IJ4JLogger>? loggerFactory = null
        //)
        //{
        //    ID = targetID;
        //    _converters = converters;
        //    _options = options;
        //    _parseConfig = parseConfig;
        //    _errors = errors;
        //    _loggerFactory = loggerFactory;

        //    _logger = _loggerFactory?.Invoke();
        //    _logger?.SetLoggedType( GetType() );

        //    // if TTarget can't be created we have to abort
        //    if( !typeof(TValue).HasPublicParameterlessConstructor() )
        //        throw new ApplicationException( $"Couldn't create and instance of {typeof(TValue)}" );

        //    Value = Activator.CreateInstance<TValue>();

        //    _keyComp = parseConfig.TextComparison;

        //    //Initialize();
        //}

        public TValue Value { get; }
        public string ID { get; }
        public ReadOnlyCollection<TargetedProperty> TargetableProperties => _properties.ToList().AsReadOnly();

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
                    _keyComp,
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
                        _options,
                        property.TargetableType,
                        _loggerFactory?.Invoke() );

                //option = CreateOption(property.TargetableType);
            }
            // next condition should never be met because there should always be
            // at least one PropertyInfo object and hence one TargetedProperty
            else AddError(keys.First(), $"Final TargetedProperty is undefined");

            // determine whether we were given at least one valid, unique (i.e., so far
            // unused) key
            keys = _options.GetUniqueKeys(keys);

            if (keys.Length == 0)
                _logger?.Error("No unique keys defined for Option");

            // if something went wrong create a NullOption to return. These cannot be
            // bound to commandline parameters but serve to capture error information
            if (keys.Length == 0 || option == null || property == null)
                option = new NullOption(_options, _loggerFactory?.Invoke());

            option.AddKeys(keys);

            _options.Add(option);

            if (property != null)
                property.BoundOption = option;

            return option;

            //return FinalizeAndStoreOption( property, option, keys );
        }

        //public OptionBase BindCollection<TProp>(
        //    Expression<Func<TValue, TProp>> propertySelector,
        //    TProp defaultValue,
        //    params string[] keys )
        //    => CreateSingleOption<TProp>( propertySelector.GetPropertyPathInfo(), keys, defaultValue );

        //public OptionBase BindCollection<TProp>(
        //    Expression<Func<TValue, TProp>> propertySelector,
        //    IEnumerable<TProp> defaultValue,
        //    params string[] keys )
        //    => CreateCollectionOption<TProp>( propertySelector.GetPropertyPathInfo(), keys, defaultValue );

        //public OptionBase BindProperty(
        //    string propertyPath,
        //    object? defaultValue,
        //    params string[] keys ) => CreateSingleOption( propertyPath, keys, defaultValue );

        //public OptionBase BindPropertyCollection(
        //    string propertyPath,
        //    params string[] keys ) => CreateCollectionOption( propertyPath, keys );

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
            _errors.AddError( this, key, error );
        }

        object IBindingTarget.GetValue()
        {
            return Value;
        }

        //private void Initialize()
        //{
        //    var type = typeof(TValue);

        //    _logger?.Verbose( "Finding targetable properties for {type}", type );

        //    ScanProperties( type, Value, new Stack<PropertyInfo>() );
        //}

        //private void ScanProperties<TScan>( Type containerType, TScan container, Stack<PropertyInfo> pathToContainer )
        //{
        //    foreach( var property in containerType.GetProperties() )
        //    {
        //        var curTP = TargetedProperty.Create( property, container, _keyComp, pathToContainer, _logger );

        //        if( !curTP.IsTargetable )
        //        {
        //            _logger?.Verbose<string>( "Property {0} is not targetable", property.Name );
        //            continue;
        //        }

        //        _properties.Add( curTP );

        //        // if the property isn't defined in the container, create it so we can 
        //        // traverse any properties it may have
        //        object? child = null;

        //        if( curTP.IsDefined )
        //        {
        //            child = property.GetValue( container );
        //        }
        //        else
        //        {
        //            // we only need to create properties that are SingleValues because
        //            // those are the only ones we recurse into looking for targetable properties
        //            if( curTP.Multiplicity == PropertyMultiplicity.SingleValue )
        //            {
        //                child = Activator.CreateInstance( property.PropertyType );
        //                property.SetValue( container, child );
        //            }
        //        }

        //        _logger?.Information<string>( "Found targetable property {0}", property.Name );

        //        // recurse over any child properties of the current property provided it's a
        //        // SingleValue property but not a ValueType (which don't have child properties)
        //        if( curTP.Multiplicity == PropertyMultiplicity.SingleValue
        //            && !typeof(ValueType).IsAssignableFrom( property.PropertyType ) )
        //        {
        //            _logger?.Verbose<string>( "Finding targetable properties and methods for {0}", property.Name );

        //            pathToContainer.Push( property );

        //            ScanProperties( property.PropertyType, child, pathToContainer );
        //        }
        //    }

        //    if( pathToContainer.Count > 0 )
        //        pathToContainer.Pop();
        //}

        //// Creates an Option based on a supported single value property
        //// The properties must be ordered from "root" to "leaf" (i.e., from the ultimate parent property to the property
        //// being targeted).
        //private OptionBase CreateSingleOption( TargetedProperty property, string[] keys )
        //{
        //    OptionBase? retVal = null;

        //    //if( property == null )
        //    //{
        //    //    _logger?.Error<string>(
        //    //        "Attempted to bind to complex property '{0}', which is not supported", propertyPath );

        //    //    property = new TargetedProperty(_keyComp, _properties.Count + 1 );
        //    //    _properties.Add( property );

        //    //    var key = keys.FirstOrDefault() ?? "?";
        //    //    AddError(key, $"Attempted to bind to complex property '{propertyPath}', which is not supported");

        //    //    return FinalizeAndStoreOption(property, retVal, keys);
        //    //}

        //    // check that it's the right multiplicity (this method only handles single-valued properties)
        //    if (!property.Multiplicity.IsTargetableSingleValue())
        //        _logger?.Error<string>( "Property '{propertyPath}' is not single-valued", property.FullPath );
        //    else
        //        retVal = CreateOption( property.PropertyInfo.PropertyType );

        //    return FinalizeAndStoreOption( property, retVal, keys );
        //}

        //// Creates an Option based on a supported collection property
        //// The properties must be ordered from "root" to "leaf" (i.e., from the ultimate parent property to the property
        //// being targeted).
        //private OptionBase CreateCollectionOption(TargetedProperty property, string[] keys )
        //{
        //    OptionBase? retVal = null;

        //    //var property = TargetedProperty.Create(properties, Value, _keyComp, _logger);
        //    //_properties.Add(property);

        //    //if ( property == null )
        //    //{
        //    //    _logger?.Error<string>(
        //    //        "Attempted to bind to complex property '{0}', which is not supported", propertyPath);

        //    //    property = new TargetedProperty(_keyComp, _properties.Count + 1);
        //    //    _properties.Add(property);

        //    //    var key = keys.FirstOrDefault() ?? "?";
        //    //    AddError(key, $"Attempted to bind to complex property '{propertyPath}', which is not supported");

        //    //    return FinalizeAndStoreOption(property, retVal, keys);
        //    //}

        //    // check that it's the right multiplicity (this method only handles collection properties)
        //    if ( !property.Multiplicity.IsTargetableCollection() )
        //        _logger?.Error<string>( "Property '{propertyPath}' is not a supported collection", property.FullPath );
        //    else
        //    {
        //        // we need to find the Type on which the collection is based
        //        var elementType = property.Multiplicity == PropertyMultiplicity.Array
        //            ? property.PropertyInfo.PropertyType.GetElementType()
        //            : property.PropertyInfo.PropertyType.GenericTypeArguments.First();

        //        retVal = CreateOption( elementType! );
        //    }

        //    return FinalizeAndStoreOption( property, retVal, keys );
        //}

        //// creates an option for the specified Type provided an ITextConverter for
        //// that Type exists. Otherwise, returns a NullOption
        //private OptionBase? CreateOption( ITargetableType targetedType ) =>
        //    targetedType.Converter == null
        //        ? null
        //        : new Option(
        //            _options,
        //            targetedType.Converter,
        //            targetedType,
        //            _loggerFactory?.Invoke() );

        //// ensures option exists and has at least one valid, unique key.
        //// returns a NullOption if not. Stores the new option in the options
        //// collection.
        //private OptionBase FinalizeAndStoreOption( TargetedProperty? property, OptionBase? option, string[] keys )
        //{
        //    keys = _options.GetUniqueKeys( keys );

        //    if( keys.Length == 0 )
        //        _logger?.Error( "No unique keys defined for Option" );

        //    if( keys.Length == 0 || option == null || property == null )
        //        option = new NullOption( _options, _loggerFactory?.Invoke() );

        //    option.AddKeys( keys );

        //    _options.Add( option );

        //    if( property != null )
        //        property.BoundOption = option;

        //    return option;
        //}
    }
}