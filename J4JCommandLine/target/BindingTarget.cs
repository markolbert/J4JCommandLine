using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.CommandLine
{
    public class BindingTarget<TTarget> : IBindingTarget<TTarget> 
        where TTarget : class
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
            TTarget target,
            IEnumerable<ITextConverter> converters,
            IOptionCollection options,
            IParsingConfiguration parseConfig,
            CommandLineErrors errors,
            Func<IJ4JLogger>? loggerFactory = null
        )
        {
            ID = targetID;
            Target = target;
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
            if( !typeof(TTarget).HasPublicParameterlessConstructor() )
                throw new ApplicationException( $"Couldn't create and instance of {typeof(TTarget)}" );

            Target = Activator.CreateInstance<TTarget>();

            _keyComp = parseConfig.TextComparison;

            Initialize();
        }

        public TTarget Target { get; }
        public string ID { get; }

        public IOption<TProp>? BindProperty<TProp>(
            Expression<Func<TTarget, TProp>> propertySelector,
            TProp defaultValue,
            params string[] keys )
        {
            var propPath = propertySelector.GetPropertyPath();

            if( _properties.Contains( propPath ) )
            {
                var converter = _converters.FirstOrDefault( c => c.SupportedType == typeof(TProp) )
                    as ITextConverter<TProp>;

                if( converter == null )
                {
                    _logger?.Error<Type>( "No ITextConverter exists for Type {0}", typeof(TProp) );
                    return null;
                }

                var option = new Option<TProp>( _options, converter, _errors, _loggerFactory?.Invoke() );
                option.AddKeys( keys );
                
                _properties[ propPath ].BoundOption = option;

                return option;
            }

            _logger?.Error<string>( "Property '{propPath}' is not bindable", propPath );

            return null;
        }

        public bool MapParseResults( ParseResults parseResults )
        {
            var retVal = true;

            foreach( var boundProp in _properties )
            {
                if( boundProp.BoundOption == null )
                    continue;

                var parseResult = parseResults
                    .FirstOrDefault( pr =>
                        boundProp.BoundOption.Keys.Any( k => string.Equals( k, pr.Key, _keyComp ) ) );

                if( boundProp.BoundOption.Convert( this, parseResult, out var result ) != TextConversionResult.Okay )
                {
                    retVal = false;
                    continue;
                }

                if( !boundProp.BoundOption.Validate( this, parseResult.Key, result ) )
                {
                    retVal = false;
                    continue;
                }

                boundProp.PropertyInfo.SetValue( Target, result );
            }

            return retVal;
        }

        public void AddError( string key, string error ) => _errors.AddError( this, key, error );

        private void Initialize()
        {
            var type = typeof(TTarget);

            _logger?.Verbose<Type>( "Finding targetable properties for {type}", type );

            ScanProperties( type, Target );
        }

        private void ScanProperties<TScan>( Type containerType, TScan container )
        {
            foreach( var property in containerType.GetProperties() )
            {
                // we can't do anything with properties which aren't defined and we can't create
                var isDefined = property.GetValue( container ) != null;
                var isCreatable = property.PropertyType.HasPublicParameterlessConstructor();

                if ( !isDefined && !isCreatable )
                {
                    _logger?.Verbose<string>( "Property {0} is not defined and not creatable, and so can't be targeted",
                        property.Name );
                    continue;
                }

                if( !property.IsPublicReadWrite( _logger ) )
                {
                    _logger?.Verbose<string>(
                        "Property {0} is not publicly readable and writable and so can't be targeted",
                        property.Name );
                    continue;
                }

                _properties.Add(new TargetableProperty(property));

                // if the property isn't defined in the container, create it so we can 
                // traverse any properties it may have
                object? child;

                if( isDefined ) child = property.GetValue( container );
                else
                {
                    child = Activator.CreateInstance( property.PropertyType );
                    property.SetValue(container, child );
                }

                _logger?.Information<string>( "Found targetable property {0}", property.Name );

                // recurse over any child properties of the current property
                _logger?.Verbose<Type>( "Finding targetable properties and methods for {0}",
                    property.PropertyType );

                ScanProperties( property.PropertyType, child );
            }
        }
    }
}