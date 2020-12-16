using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class Options : IEnumerable<Option>
    {
        private readonly ITypeInitializer _typeInitializer;
        private readonly List<Option> _options = new List<Option>();
        private readonly MasterTextCollection _masterText;

        public Options( 
            ITypeInitializer typeInitializer,
            MasterTextCollection masterText,
            IJ4JLogger logger
            )
        {
            _typeInitializer = typeInitializer;
            _masterText = masterText;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool UsesCommandLineKey( string key ) => _masterText.Contains( key, TextUsageType.OptionKey );

        public bool UsesContextPath( List<IContextKey> contextPath )
        {
            if( !_masterText.IsValid )
            {
                Logger.Error( "MasterTextCollection is not initialized" );
                return false;
            }

            var pathText = contextPath.ToTextPath();

            return _options.Any( x =>
                x.ContextPath?.ToTextPath().Equals( pathText, _masterText.TextComparison!.Value ) ?? false );
        }

        // eliminates duplicate keys from a collection based on the case sensitivity rules
        public string[] GetUniqueCommandLineKeys( params string[] keys ) => keys
            .Where( k => !UsesCommandLineKey( k ) )
            .ToArray();

        public Option? this[ string key ]
        {
            get
            {
                if (!_masterText.IsValid)
                {
                    Logger.Error("MasterTextCollection is not initialized");
                    return null;
                }

                return _options.FirstOrDefault( opt =>
                    opt.Keys.Any( k => string.Equals( k, key, _masterText.TextComparison!.Value ) ) );
            }
        }

        public Option Add(IContextKey contextKey )
        {
            var retVal = new Option( this, contextKey, _masterText );

            _options.Add(retVal);

            return retVal;
        }

        public void InitializeFromType<TTarget>()
            where TTarget: class
        {
            _options.Clear();

            AddFromPropertyKeys( _typeInitializer.GetContextKeys<TTarget>(), null );
        }

        private void AddFromPropertyKeys( List<PropertyKey> propKeys, PropertyKey? parentKey )
        {
            Option? parent = null;

            if( parentKey?.ContextPath != null )
                parent = _options.FirstOrDefault( x => x.ContextPath?.ToTextPath()
                        .Equals( parentKey.ContextPath.ToTextPath(), StringComparison.Ordinal )
                    ?? false );

            foreach ( var propKey in propKeys )
            {
                var option = Add( propKey );

                if( parent != null )
                    option.ChildOf( parent );

                AddFromPropertyKeys(
                    propKeys.Where( x => x.Parent == propKey )
                        .ToList(),
                    propKey );
            }
        }

        public IEnumerator<Option> GetEnumerator()
        {
            foreach( var option in _options ) yield return option;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}