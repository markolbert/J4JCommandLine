using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OptionCollection : IOptionCollection
    {
        private readonly List<IOption> _options = new List<IOption>();
        private readonly IJ4JLogger? _logger;
        private readonly IParsingConfiguration _parseConfig;

        public OptionCollection( 
            IParsingConfiguration parseConfig,
            IJ4JLogger? logger = null 
        )
        {
            _parseConfig = parseConfig;
            _logger = logger;

            _logger?.SetLoggedType( this.GetType() );
        }

        public ReadOnlyCollection<IOption> Options => _options.AsReadOnly();

        public bool HasKey( string key )
        {
            if( _options.Any( opt => opt.Keys.Any( k => string.Equals( k, key, _parseConfig.TextComparison ) ) ) )
            {
                _logger?.Verbose<string>( "Key '{0}' already in use", key );

                return false;
            }

            _logger?.Verbose<string>("Key '{0}' not in use", key);

            return true;
        }

        public IOption? this[ string key ] =>
                _options.FirstOrDefault( opt =>
                    opt.Keys.Any( k => string.Equals( k, key, _parseConfig.TextComparison ) ) );

        public void Clear()
        {
            _options.Clear();

            _logger?.Verbose( "Cleared option collection" );
        }

        public bool Add( IOption option )
        {
            foreach( var key in option.Keys )
            {
                if( _options.Any( opt => opt.Keys.Any( k => string.Equals( k, key, _parseConfig.TextComparison ) ) ) )
                {
                    _logger?.Warning<string>( "Key '{0}' already in use", key );

                    return false;
                }
            }

            _options.Add( option );

            _logger?.Verbose<string>( "Option '{0}' added", option.ToString() ?? "**option**" );

            return true;
        }
    }
}