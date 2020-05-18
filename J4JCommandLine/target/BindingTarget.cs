using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.CommandLine
{
    public class BindingTarget<TValue> : IBindingTarget<TValue> 
        where TValue : class
    {
        private readonly TargetableProperties _properties = new TargetableProperties();
        private readonly IJ4JLogger? _logger;
        private readonly Func<IJ4JLogger>? _loggerFactory;
        private readonly StringComparison _keyComp;
        private readonly IEnumerable<ITextConverter> _converters;
        private readonly CommandLineErrors _errors;
        private readonly IOptionCollection _options;

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
            _logger?.SetLoggedType( this.GetType() );

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
            _errors = errors;
            _loggerFactory = loggerFactory;

            _logger = _loggerFactory?.Invoke();
            _logger?.SetLoggedType( this.GetType() );

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
            TProp defaultValue,
            params string[] keys )
        {
            var propPath = propertySelector.GetPropertyPath();

            return BindProperty( propPath!, defaultValue!, keys )!;
        }

        public OptionBase BindProperty(
            string propertyPath,
            object defaultValue,
            params string[] keys)
        {
            if (_properties.Contains(propertyPath))
            {
                var propType = _properties[ propertyPath ].PropertyInfo.PropertyType;

                var converter = _converters.FirstOrDefault( c => c.SupportedType == propType );

                if (converter == null)
                {
                    _logger?.Error<Type>("No ITextConverter exists for Type {0}", propType);

                    return new NullOption( _options, _loggerFactory?.Invoke() );
                }

                var option = new Option(_options, converter, _loggerFactory?.Invoke());
                option.AddKeys(keys);
                option.SetDefaultValue(defaultValue);

                _properties[propertyPath].BoundOption = option;

                return option;
            }

            _logger?.Error<string>("Property '{propertyPath}' is not bindable", propertyPath);

            return new NullOption(_options, _loggerFactory?.Invoke());
        }

        public MappingResults MapParseResults( ParseResults parseResults )
        {
            var retVal = MappingResults.Success;

            // scan all the bound options that aren't tied to NullOptions, which are only
            // "bound" in error
            foreach( var boundProp in _properties.Where( p => p.BoundOption != null ) )
            {
                if( boundProp.BoundOption is NullOption )
                    retVal |= MappingResults.Unbound;
                else
                    retVal |= boundProp.MapParseResult( this, parseResults, _logger );
            }

            return retVal;
        }

        public void AddError( string key, string error ) => _errors.AddError( this, key, error );

        private void Initialize()
        {
            var type = typeof(TValue);

            _logger?.Verbose<Type>( "Finding targetable properties for {type}", type );

            ScanProperties( type, Value, new Stack<PropertyInfo>() );
        }

        private void ScanProperties<TScan>( Type containerType, TScan container, Stack<PropertyInfo> pathToContainer )
        {
            foreach( var property in containerType.GetProperties() )
            {
                var curTP = TargetableProperty.Create( property, container, _keyComp, pathToContainer, _logger );

                if ( !curTP.IsTargetable )
                {
                    _logger?.Verbose<string>( "Property {0} is not targetable", property.Name );
                    continue;
                }

                _properties.Add( curTP );

                // if the property isn't defined in the container, create it so we can 
                // traverse any properties it may have
                object? child = null;

                if( curTP.IsDefined ) child = property.GetValue( container );
                else
                {
                    // no need to create strings because they have no targetable subproperties
                    if( property.PropertyType != typeof(string) )
                    {
                        child = Activator.CreateInstance( property.PropertyType );
                        property.SetValue( container, child );
                    }
                }

                _logger?.Information<string>( "Found targetable property {0}", property.Name );

                // recurse over any child properties of the current property provided it's a
                // SingleValue property but not a ValueType (which don't have child properties)
                if( curTP.Multiplicity == PropertyMultiplicity.SingleValue 
                    && !typeof(ValueType).IsAssignableFrom(property.PropertyType) )
                {
                    _logger?.Verbose<string>( "Finding targetable properties and methods for {0}", property.Name );

                    pathToContainer.Push( property );

                    ScanProperties(property.PropertyType, child, pathToContainer);
                }
            }

            if( pathToContainer.Count > 0 )
                pathToContainer.Pop();
        }

        object IBindingTarget.GetValue() => Value;
    }
}