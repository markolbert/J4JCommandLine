using System;
using System.Collections.Generic;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class TargetableTypes
    {
        private readonly IJ4JLogger? _logger;
        private readonly Dictionary<Type, ITargetableType> _targetableTypes = new Dictionary<Type, ITargetableType>();

        public TargetableTypes( IJ4JLogger? logger )
        {
            _logger = logger;

            _logger?.SetLoggedType(this.GetType());
        }

        public bool Add( ITargetableType tType )
        {
            if( _targetableTypes.ContainsKey( tType.Type ) )
            {
                _logger?.Warning( "Duplicate ITargetableType for '{0}', replacing earlier item", tType.Type );
                _targetableTypes[ tType.Type ] = tType;

                return false;
            }

            _logger?.Verbose( "Added ITargetableType for '{0}'", tType.Type );
            _targetableTypes.Add( tType.Type, tType );

            return true;
        }

        public bool CanTarget( Type toTarget ) => _targetableTypes.ContainsKey( toTarget );

        public ITargetableType? Get( Type toTarget )
        {
            if( _targetableTypes.ContainsKey( toTarget ) )
                return _targetableTypes[ toTarget ];

            return null;
        }
    }
}