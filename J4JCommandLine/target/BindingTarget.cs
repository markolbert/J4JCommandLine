using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class BindingTarget<TTarget>
        where TTarget : class
    {
        private readonly Dictionary<string, TargetableMember> _properties =
            new Dictionary<string, TargetableMember>( StringComparer.Ordinal );

        private readonly Dictionary<string, TargetableMethod> _methods =
            new Dictionary<string, TargetableMethod>(StringComparer.Ordinal);

        public BindingTarget(
            ITargetingConfiguration targetingConfig,
            IJ4JLogger? logger = null
        )
        {
            TargetingConfiguration = targetingConfig;
            Logger = logger;

            Logger?.SetLoggedType( this.GetType() );
        }

        protected IJ4JLogger? Logger { get; }
        protected ITargetingConfiguration TargetingConfiguration { get; }

        public bool Initialize( TTarget target )
        {
            var type = target.GetType();

            Logger?.Verbose<Type>( "Finding targetable properties and methods for {type}", type );

            // grab the targetable properties
            ScanProperties( type.GetProperties() );

            // grab the root targetable methods
            ScanMethods( type );

            return true;
        }

        protected void ScanProperties( IEnumerable<PropertyInfo> properties )
        {
            foreach( var property in properties )
            {
                // we can't do anything with properties we can't create
                if( !TargetingConfiguration.CanCreate( property.PropertyType ) )
                {
                    Logger?.Verbose<string>( "Property {0} is not creatable and can't be targeted", property.Name );
                    continue;
                }

                // grab the targetable methods
                ScanMethods( property.PropertyType );

                var canTarget = TargetingConfiguration.CanTarget( property.PropertyType );

                if( canTarget  )
                {
                    if( !property.IsPublicReadWrite( Logger ) )
                    {
                        Logger?.Verbose<string>("Property {0} is not publicly readable and writable and so can't be targeted", property.Name);
                        continue;
                    }

                    var newTP = new TargetableMember( property );
                    _properties.Add( newTP.Path, newTP );

                    Logger?.Information<string>("Found targetable property {0}", property.Name);
                }
                else
                {
                    // recurse over any child properties if we can't directly target the 
                    // current property
                    Logger?.Verbose<Type>("Finding targetable properties and methods for {0}", property.PropertyType);

                    ScanProperties( property.PropertyType.GetProperties() );
                }
            }
        }

        protected void ScanMethods( Type type )
        {
            foreach (var method in type.GetMethods())
            {
                Logger?.Verbose<string>( "Determining if {0}() is targetable", method.Name );

                var parameters = method.GetParameters();
                var allTargetable = true;

                foreach (var parameter in parameters)
                {
                    // we can't do anything with parameters we can't create and target
                    if( TargetingConfiguration.CanCreate( parameter.ParameterType )
                        && TargetingConfiguration.CanTarget( parameter.ParameterType ) )
                    {
                        Logger?.Verbose<string>( "Parameter {0} is not creatable and targetable",
                            parameter.Name ?? "**unnamed parameter**" );
                        continue;
                    }

                    allTargetable = false;
                    break;
                }

                if (allTargetable)
                {
                    var newMethod = new TargetableMethod(method)
                    {
                        Parameters = parameters.ToList()
                    };

                    _methods.Add(newMethod.Path, newMethod);

                    Logger?.Information<string>( "Found targetable method {0}()", method.Name );
                }
            }
        }
    }
}