using System;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.CommandLine
{
    public class OptionFactory : IOptionFactory
    {
        private readonly IServiceProvider _svcProvider;
        private readonly IOptionCollection _options;
        private readonly IJ4JLogger? _logger;

        public OptionFactory( 
            IServiceProvider svcProvider,
            IOptionCollection options,
            IJ4JLogger? logger 
        )
        {
            _svcProvider = svcProvider;
            _options = options;
            _logger = logger;

            _logger?.SetLoggedType( this.GetType() );
        }

        public IOption<T>? CreateOption<T>( params string[] keys )
        {
            if( keys == null || keys.Length == 0 )
            {
                _logger?.Fatal<Type>( "No option keys provided for creating an instance of {0}", typeof(Option<T>) );
                return null;
            }

            var dupeKeys = keys.Where( k => _options.HasKey( k ) )
                .ToList();

            var validKeys = keys.Except( dupeKeys )
                .ToList();

            if( dupeKeys.Count > 0 )
            {
                if( validKeys.Count == 0 )
                {
                    _logger?.Error<Type>("No valid keys for creating an instance of {0}", typeof(Option<T>));
                    return null;
                }

                _logger?.Error<string, Type>( 
                    "Duplicate keys '{0}' ignored in creating an instance of {0}",
                    string.Join( ", ", dupeKeys ), 
                    typeof(Option<T>) );
            }

            try
            {
                var retVal = _svcProvider.GetRequiredService<Option<T>>();
                _logger?.Verbose<Type>( "Created {0}", typeof(Option<T>) );

                retVal.AddKeys( validKeys );

                _options.Add( retVal );

                return retVal;
            }
            catch( Exception e )
            {
                _logger?.Fatal<Type>( "Unable to create {0}", typeof(Option<T>) );

                return null;
            }
        }
    }
}