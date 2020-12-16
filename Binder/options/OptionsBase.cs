using System.Collections;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.CommandLine
{
    public class OptionsBase : IEnumerable<Option>
    {
        private readonly List<Option> _options = new List<Option>();
        private readonly MasterTextCollection _masterText;

        protected OptionsBase( 
            MasterTextCollection masterText,
            IJ4JLogger logger
        )
        {
            _masterText = masterText;

            Logger = logger;
            Logger.SetLoggedType( GetType() );
        }

        protected IJ4JLogger Logger { get; }

        // determines whether or not a key is being used by an existing option, honoring whatever
        // case sensitivity is in use
        public bool UsesCommandLineKey( string key ) => _masterText.Contains( key, TextUsageType.OptionKey );

        public bool UsesContextPath( string contextPath )
        {
            if( !_masterText.IsValid )
            {
                Logger.Error( "MasterTextCollection is not initialized" );
                return false;
            }

            return _options.Any( x =>
                x.ContextPath?.Equals( contextPath, _masterText.TextComparison!.Value ) ?? false );
        }

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

        protected Option AddInternal(string contextPath )
        {
            var retVal = new Option( this, contextPath, _masterText );

            _options.Add(retVal);

            return retVal;
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