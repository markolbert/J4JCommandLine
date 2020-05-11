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
    public class BindingTarget<TTarget> 
        where TTarget : class
    {
        private readonly TargetableProperties _properties = new TargetableProperties();
        private readonly ITargetingConfiguration _config;
        private readonly IOptionFactory _optionFactory;
        private readonly IJ4JLogger? _logger;

        private TTarget _target;

        public BindingTarget(
            TTarget target,
            ITargetingConfiguration config,
            IOptionFactory optionFactory,
            IJ4JLogger? logger = null
        )
        {
            _target = target;
            _config = config;
            _optionFactory = optionFactory;
            _logger = logger;

            _logger?.SetLoggedType(this.GetType());

            Initialize( _target );
        }

        public BindingTarget(
            ITargetingConfiguration config,
            IOptionFactory optionFactory,
            IJ4JLogger? logger = null
        )
        {
            _config = config;
            _optionFactory = optionFactory;
            _logger = logger;

            _logger?.SetLoggedType( this.GetType() );

            // if TTarget can't be created on the fly we have to abort
            // first see if it has a parameterless constructor...
            var ctor = typeof(TTarget).GetConstructor( Type.EmptyTypes );

            if( ctor != null )
                _target = Activator.CreateInstance<TTarget>();

            if( _target == null )
            {
                if( config.ServiceProvider == null )
                    throw new ApplicationException(
                        $"{typeof(TTarget)} has no parameterless constructor and no {nameof(IServiceProvider)} was defined" );

                try
                {
                    _target = config.ServiceProvider.GetRequiredService<TTarget>();
                }
                catch( Exception e )
                {
                    throw new ApplicationException(
                        $"Couldn't create and instance of {typeof(TTarget)} from {nameof(IServiceProvider)}, see inner exception for details",
                        e );
                }
            }

            Initialize( _target );
        }

        public TTarget Target => _target;

        public IOption<TProp>? BindProperty<TProp>(
            Expression<Func<TTarget, TProp>> propertySelector, 
            params string[] keys)
        {
            var propPath = propertySelector.GetPropertyPath();

            if (_properties.Contains(propPath))
            {
                var option = _optionFactory.CreateOption<TProp>( keys );

                if( option == null )
                    return null;

                _properties[ propPath ].BoundOption = option;
            }

            _logger?.Error<string>("Property '{propPath}' is not bindable", propPath);

            return null;
        }

        public bool Bind( List<IParseResult> parseResults )
        {
            var retVal = true;

            foreach( var boundProp in _properties )
            {
                if( boundProp.BoundOption == null )
                    continue;

                var textElements = parseResults.FirstOrDefault( pr => boundProp.BoundOption.Keys.Any( k => string.Equals( k, pr.Key ) ) )
                    ?.Arguments ?? null;

                var convResult = boundProp.BoundOption.Convert( textElements, out var result, out var error );

                switch( convResult )
                {
                    case TextConversionResult.Okay:
                        boundProp.PropertyInfo.SetValue( Target, result );
                        break;

                    default:
                        retVal = false;
                        break;
                }
            }

            return retVal;
        }

        private void Initialize(TTarget target)
        {
            var type = typeof(TTarget);

            _logger?.Verbose<Type>("Finding targetable properties for {type}", type);

            ScanProperties(type.GetProperties());
        }

        private void ScanProperties( IEnumerable<PropertyInfo> properties )
        {
            foreach( var property in properties )
            {
                // we can't do anything with properties we can't create
                if( !_config.CanCreate( property.PropertyType ) )
                {
                    _logger?.Verbose<string>( "Property {0} is not creatable and can't be targeted", property.Name );
                    continue;
                }

                if( _config.CanTarget( property.PropertyType ) )
                {
                    if( !property.IsPublicReadWrite( _logger ) )
                    {
                        _logger?.Verbose<string>(
                            "Property {0} is not publicly readable and writable and so can't be targeted",
                            property.Name );
                        continue;
                    }

                    _properties.Add( new TargetableProperty( property ) );

                    _logger?.Information<string>( "Found targetable property {0}", property.Name );
                }
                else
                {
                    // recurse over any child properties if we can't directly target the 
                    // current property
                    _logger?.Verbose<Type>( "Finding targetable properties and methods for {0}", property.PropertyType );

                    ScanProperties( property.PropertyType.GetProperties() );
                }
            }
        }
    }
}